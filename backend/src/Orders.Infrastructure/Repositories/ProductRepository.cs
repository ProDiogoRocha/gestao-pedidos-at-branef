
using Microsoft.EntityFrameworkCore;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Products;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly OrdersDbContext _db;

    public ProductRepository(OrdersDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
        => await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken ct)
        => await _db.Products.OrderBy(p => p.Name).ToListAsync(ct);
}
