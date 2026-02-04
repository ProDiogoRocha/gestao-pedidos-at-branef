
using MediatR;
using Orders.Application.Orders.Models;

namespace Orders.Application.Orders.Commands;

public sealed record CreateOrderCommand(List<OrderItemRequest> Items) : IRequest<Guid>;

public sealed record UpdateOrderCommand(Guid Id, List<OrderItemRequest> Items, int Status) : IRequest<bool>;

public sealed record DeleteOrderCommand(Guid Id) : IRequest<bool>;
