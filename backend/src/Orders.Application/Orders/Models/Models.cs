
namespace Orders.Application.Orders.Models;

public sealed record OrderItemRequest(Guid ProductId, int Quantity);

public sealed record CreateOrderRequest(List<OrderItemRequest> Items);

public sealed record UpdateOrderRequest(List<OrderItemRequest> Items, int Status);

public sealed record ProductDto(Guid Id, string Name, decimal Price);

public sealed record OrderItemReadDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal TotalPrice);

public sealed record OrderReadDto(
    Guid Id,
    Guid CustomerId,
    DateTime OrderDate,
    decimal TotalAmount,
    int Status,
    List<OrderItemReadDto> Items
);

public sealed record CreateOrderResponse(Guid Id);

public sealed record OperationResultDto(bool Ok);
