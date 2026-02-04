
using MediatR;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Models;
using Orders.Application.Orders.Queries;

namespace Orders.Application.Orders.Handlers;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderReadDto?>
{
    private readonly IOrderReadRepository _read;
    public GetOrderByIdQueryHandler(IOrderReadRepository read) => _read = read;

    public Task<OrderReadDto?> Handle(GetOrderByIdQuery request, CancellationToken ct)
        => _read.GetByIdAsync(request.Id, ct);
}

public sealed class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, IReadOnlyList<OrderReadDto>>
{
    private readonly IOrderReadRepository _read;
    public ListOrdersQueryHandler(IOrderReadRepository read) => _read = read;

    public Task<IReadOnlyList<OrderReadDto>> Handle(ListOrdersQuery request, CancellationToken ct)
        => _read.ListAsync(ct);
}

public sealed class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _products;
    public ListProductsQueryHandler(IProductRepository products) => _products = products;

    public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery request, CancellationToken ct)
    {
        var list = await _products.ListAsync(ct);
        return list.Select(p => new ProductDto(p.Id, p.Name, p.Price)).ToList();
    }
}
