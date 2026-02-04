
using MongoDB.Driver;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Models;
using Orders.Infrastructure.Persistence.Mongo;

namespace Orders.Infrastructure.Repositories;

public sealed class OrderReadRepository : IOrderReadRepository
{
    private readonly MongoContext _mongo;
    public OrderReadRepository(MongoContext mongo) => _mongo = mongo;

    public async Task<OrderReadDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var cursor = await _mongo.Orders.FindAsync(x => x.Id == id, cancellationToken: ct);
        return await cursor.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<OrderReadDto>> ListAsync(CancellationToken ct)
    {
        var cursor = await _mongo.Orders.FindAsync(_ => true, cancellationToken: ct);
        return await cursor.ToListAsync(ct);
    }
}
