
using Microsoft.EntityFrameworkCore;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Orders;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Repositories;

public sealed class OrderWriteRepository : IOrderWriteRepository
{
    private readonly OrdersDbContext _db;

    public OrderWriteRepository(OrdersDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Orders.AsTracking().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct)
        => await _db.Orders.AddAsync(order, ct);

    public void Remove(Order order) => _db.Orders.Remove(order);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
