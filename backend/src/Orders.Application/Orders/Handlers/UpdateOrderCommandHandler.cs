
using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Models;
using Orders.Domain.Orders;

namespace Orders.Application.Orders.Handlers;

public sealed class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, bool>
{
    private readonly IProductRepository _products;
    private readonly IOrderWriteRepository _orders;
    private readonly IOrderProjectionWriter _projectionWriter;
    private readonly ILogger<UpdateOrderCommandHandler> _logger;

    public UpdateOrderCommandHandler(
        IProductRepository products,
        IOrderWriteRepository orders,
        IOrderProjectionWriter projectionWriter,
        ILogger<UpdateOrderCommandHandler> logger)
    {
        _products = products;
        _orders = orders;
        _projectionWriter = projectionWriter;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Iniciando atualização do pedido {OrderId}", request.Id);
            
            var order = await _orders.GetByIdAsync(request.Id, ct);
            if (order is null)
            {
                _logger.LogWarning("Pedido {OrderId} não encontrado", request.Id);
                return false;
            }

            var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
            _logger.LogInformation("Buscando {ProductCount} produtos", productIds.Count);
            
            var products = await _products.GetByIdsAsync(productIds, ct);
            _logger.LogInformation("Encontrados {ProductCount} produtos", products.Count);
            
            if (products.Count != productIds.Count)
            {
                var notFound = productIds.Except(products.Select(p => p.Id)).FirstOrDefault();
                var error = $"Produto não encontrado: {notFound}";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            var productsById = products.ToDictionary(p => p.Id, p => p);
            
            order.ReplaceItems(request.Items.Select(i => (productsById[i.ProductId], i.Quantity)));
            order.SetStatus((OrderStatus)request.Status);

            await _orders.SaveChangesAsync(ct);
            _logger.LogInformation("Pedido {OrderId} salvo com sucesso", request.Id);

            try
            {
                var snapshot = new OrderReadDto(
                    order.Id,
                    order.CustomerId,
                    order.OrderDate,
                    order.TotalAmount,
                    (int)order.Status,
                    order.Items.Select(i => new OrderItemReadDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
                );
                await _projectionWriter.UpsertAsync(snapshot, ct);
                _logger.LogInformation("Projeção MongoDB atualizada para pedido {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao atualizar projeção MongoDB para pedido {OrderId}. Sincronização será feita pelo Outbox.", order.Id);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar pedido {OrderId}", request.Id);
            throw;
        }
    }
}
