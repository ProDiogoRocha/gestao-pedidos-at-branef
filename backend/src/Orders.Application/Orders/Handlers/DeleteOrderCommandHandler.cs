
using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Commands;

namespace Orders.Application.Orders.Handlers;

public sealed class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly IOrderWriteRepository _orders;
    private readonly IOrderProjectionWriter _projectionWriter;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(
        IOrderWriteRepository orders,
        IOrderProjectionWriter projectionWriter,
        ILogger<DeleteOrderCommandHandler> logger)
    {
        _orders = orders;
        _projectionWriter = projectionWriter;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(request.Id, ct);
        if (order is null) return false;

        _orders.Remove(order);
        await _orders.SaveChangesAsync(ct);

        try
        {
            await _projectionWriter.DeleteAsync(request.Id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao deletar projeção MongoDB para pedido {OrderId}. Sincronização será feita pelo Outbox.", request.Id);
        }

        return true;
    }
}
