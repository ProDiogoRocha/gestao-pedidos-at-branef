
using Orders.Domain.Common;
using Orders.Domain.Products;

namespace Orders.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public Order(Guid id, Guid customerId, DateTime orderDateUtc)
    {
        Id = id;
        CustomerId = customerId;
        OrderDate = DateTime.SpecifyKind(orderDateUtc, DateTimeKind.Utc);
        Status = OrderStatus.Created;
        RecalculateTotals();
    }

    public void AddOrUpdateItem(Product product, int quantity)
    {
        Guard.AgainstNull(product, nameof(product));
        Guard.AgainstNonPositive(quantity, nameof(quantity));

        var existing = _items.FirstOrDefault(x => x.ProductId == product.Id);
        if (existing is null)
        {
            _items.Add(new OrderItem(Guid.NewGuid(), Id, product.Id, product.Name, quantity, product.Price));
        }
        else
        {
            existing.SetQuantity(quantity);
        }

        RecalculateTotals();
    }

    public void RemoveItem(Guid productId)
    {
        var existing = _items.FirstOrDefault(x => x.ProductId == productId);
        if (existing is null) return;

        _items.Remove(existing);
        RecalculateTotals();
    }

    public void SetStatus(OrderStatus status)
    {
        Status = status;
    }

    public void ReplaceItems(IEnumerable<(Product product, int quantity)> items)
    {
        Guard.AgainstNull(items, nameof(items));

        var newItems = items.ToList();
        var newProductIds = newItems.Select(i => i.product.Id).ToHashSet();

        // Remove items que não estão mais na lista
        var itemsToRemove = _items.Where(i => !newProductIds.Contains(i.ProductId)).ToList();
        foreach (var item in itemsToRemove)
        {
            _items.Remove(item);
        }

        // Adicionar ou atualizar items
        foreach (var (product, quantity) in newItems)
        {
            AddOrUpdateItem(product, quantity);
        }

        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        TotalAmount = _items.Sum(i => i.TotalPrice);
    }
}
