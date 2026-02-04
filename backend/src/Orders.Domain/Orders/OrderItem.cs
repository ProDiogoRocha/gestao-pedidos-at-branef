
using Orders.Domain.Common;

namespace Orders.Domain.Orders;

public sealed class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    public Order? Order { get; private set; }
    private OrderItem() { }

    internal OrderItem(Guid id, Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice)
    {
        Guard.AgainstEmpty(productName, nameof(productName));
        Guard.AgainstNonPositive(quantity, nameof(quantity));
        Guard.AgainstNegative(unitPrice, nameof(unitPrice));

        Id = id;
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice * quantity;
    }

    internal void SetQuantity(int quantity)
    {
        Guard.AgainstNonPositive(quantity, nameof(quantity));
        Quantity = quantity;
        TotalPrice = UnitPrice * Quantity;
    }
}
