
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Domain.Customers;
using Orders.Domain.Products;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Services;

public sealed class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<DatabaseInitializerHostedService> _logger;
    private readonly IConfiguration _configuration;

    public DatabaseInitializerHostedService(IServiceProvider provider, ILogger<DatabaseInitializerHostedService> logger, IConfiguration configuration)
    {
        _provider = provider;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        _logger.LogInformation("Criando banco (EnsureCreated)...");
        await db.Database.EnsureCreatedAsync(cancellationToken);

        var fixedCustomerId = Guid.Parse(_configuration["FixedCustomer:Id"]!);

        if (!await db.Customers.AnyAsync(c => c.Id == fixedCustomerId, cancellationToken))
        {
            db.Customers.Add(new Customer(
                fixedCustomerId,
                "Cliente Fixo",
                "cliente.fixo@exemplo.com",
                "+55 11 99999-9999"
            ));
        }

        var products = new[]
        {
            new Product(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Camiseta", 59.90m),
            new Product(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Calça", 129.90m),
            new Product(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Tênis", 249.90m),
        };

        foreach (var p in products)
        {
            if (!await db.Products.AnyAsync(x => x.Id == p.Id, cancellationToken))
                db.Products.Add(p);
        }

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seed concluído.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
