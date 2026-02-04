
using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Models;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.Handlers;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IProductRepository _products;
    private readonly IOrderWriteRepository _orders;
    private readonly IFixedCustomerProvider _fixedCustomer;
    private readonly IOrderProjectionWriter _projectionWriter;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IProductRepository products,
        IOrderWriteRepository orders,
        IFixedCustomerProvider fixedCustomer,
        IOrderProjectionWriter projectionWriter,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _products = products;
        _orders = orders;
        _fixedCustomer = fixedCustomer;
        _projectionWriter = projectionWriter;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _products.GetByIdsAsync(productIds, ct);

        // garante que todos existem
        var productsById = products.ToDictionary(p => p.Id, p => p);
        foreach (var item in request.Items)
        {
            if (!productsById.ContainsKey(item.ProductId))
                throw new InvalidOperationException($"Produto não encontrado: {item.ProductId}");
        }

        var order = new Order(Guid.NewGuid(), _fixedCustomer.GetCustomerId(), DateTime.UtcNow);

        foreach (var item in request.Items)
        {
            var product = productsById[item.ProductId];
            order.AddOrUpdateItem(product, item.Quantity);
        }

        await _orders.AddAsync(order, ct);
        await _orders.SaveChangesAsync(ct);

        // tentativa de projeção imediata (falha aqui não quebra a escrita; Outbox garante eventual)
        try
        {
            var snapshot = ToReadDto(order);
            await _projectionWriter.UpsertAsync(snapshot, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao atualizar projeção MongoDB para pedido {OrderId}. Sincronização será feita pelo Outbox.", order.Id);
        }

        return order.Id;
    }

    private static OrderReadDto ToReadDto(Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.OrderDate,
            order.TotalAmount,
            (int)order.Status,
            order.Items.Select(i => new OrderItemReadDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
        );
}
