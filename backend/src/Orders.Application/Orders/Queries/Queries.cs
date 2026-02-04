
using MediatR;
using Orders.Application.Orders.Models;

namespace Orders.Application.Orders.Queries;

public sealed record GetOrderByIdQuery(Guid Id) : IRequest<OrderReadDto?>;

public sealed record ListOrdersQuery() : IRequest<IReadOnlyList<OrderReadDto>>;

public sealed record ListProductsQuery() : IRequest<IReadOnlyList<ProductDto>>;
