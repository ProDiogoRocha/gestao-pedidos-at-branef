using MongoDB.Driver;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Models;
using Orders.Infrastructure.Persistence.Mongo;

namespace Orders.Infrastructure.Repositories;

public sealed class OrderProjectionWriter : IOrderProjectionWriter
{
    private readonly MongoContext _mongo;

    public OrderProjectionWriter(MongoContext mongo) => _mongo = mongo;

    public Task UpsertAsync(OrderReadDto snapshot, CancellationToken ct)
    {
        var filter = Builders<OrderReadDto>.Filter.Eq(x => x.Id, snapshot.Id);
        return _mongo.Orders.ReplaceOneAsync(
            filter,
            snapshot,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var filter = Builders<OrderReadDto>.Filter.Eq(x => x.Id, id);
        return _mongo.Orders.DeleteOneAsync(filter, ct);
    }
}
