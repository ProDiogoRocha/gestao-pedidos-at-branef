
namespace Orders.Domain.Orders;

public enum OrderStatus
{
    Created = 0,
    Paid = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
