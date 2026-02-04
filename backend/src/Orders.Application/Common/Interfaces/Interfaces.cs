
using Orders.Application.Orders.Models;
using Orders.Domain.Orders;
using Orders.Domain.Products;

namespace Orders.Application.Common.Interfaces;

public interface IOrderWriteRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    void Remove(Order order);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
    Task<IReadOnlyList<Product>> ListAsync(CancellationToken ct);
}

public interface IOrderReadRepository
{
    Task<OrderReadDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<OrderReadDto>> ListAsync(CancellationToken ct);
}

public interface IOrderProjectionWriter
{
    Task UpsertAsync(OrderReadDto snapshot, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public interface IFixedCustomerProvider
{
    Guid GetCustomerId();
}
