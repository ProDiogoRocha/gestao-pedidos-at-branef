using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Application.Orders.Models;
using Orders.Infrastructure.Persistence.Mongo;

namespace Orders.Infrastructure.Services;

public sealed class OutboxProcessorHostedService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OutboxProcessorHostedService> _logger;
    private readonly MongoContext _mongo;

    public OutboxProcessorHostedService(IConfiguration configuration, ILogger<OutboxProcessorHostedService> logger, MongoContext mongo)
    {
        _configuration = configuration;
        _logger = logger;
        _mongo = mongo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatch(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processamento do Outbox.");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task ProcessBatch(CancellationToken ct)
    {
        var cs = _configuration.GetConnectionString("SqlServer");
        await using var conn = new SqlConnection(cs);
        await conn.OpenAsync(ct);

        const string selectSql = @"
SELECT TOP (20)
    Id, Type, Payload
FROM OutboxMessages WITH (READPAST)
WHERE ProcessedOnUtc IS NULL
ORDER BY OccurredOnUtc;";

        var messages = (await conn.QueryAsync<OutboxRow>(selectSql)).ToList();
        if (messages.Count == 0) return;

        foreach (var msg in messages)
        {
            try
            {
                switch (msg.Type)
                {
                    case "OrderCreated":
                    case "OrderUpdated":
                    {
                        var snapshot = JsonSerializer.Deserialize<OrderReadDto>(msg.Payload);
                        if (snapshot is not null)
                        {
                            await _mongo.Orders.ReplaceOneAsync(
                                MongoDB.Driver.Builders<OrderReadDto>.Filter.Eq("Id", snapshot.Id),
                                snapshot,
                                new MongoDB.Driver.ReplaceOptions { IsUpsert = true },
                                ct);
                        }
                        break;
                    }
                    case "OrderDeleted":
                    {
                        var id = JsonDocument.Parse(msg.Payload).RootElement.GetProperty("Id").GetGuid();
                        var filter = MongoDB.Driver.Builders<OrderReadDto>.Filter.Eq("Id", id);
                        await _mongo.Orders.DeleteOneAsync(filter, ct);
                        break;
                    }
                }

                const string markSql = @"
UPDATE OutboxMessages
SET ProcessedOnUtc = SYSUTCDATETIME(), Error = NULL
WHERE Id = @Id;";

                await conn.ExecuteAsync(markSql, new { msg.Id });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao processar OutboxMessage {OutboxId}", msg.Id);

                const string errSql = @"
UPDATE OutboxMessages
SET RetryCount = RetryCount + 1, Error = @Error
WHERE Id = @Id;";

                await conn.ExecuteAsync(errSql, new { msg.Id, Error = ex.ToString() });
            }
        }
    }

    private sealed class OutboxRow
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}
