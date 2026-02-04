
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Models;
using Orders.Application.Orders.Queries;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IReadOnlyList<OrderReadDto>> List(CancellationToken ct)
        => await _mediator.Send(new ListOrdersQuery(), ct);

    [HttpGet("{id:guid}")]
    public async Task<OrderReadDto?> Get(Guid id, CancellationToken ct)
        => await _mediator.Send(new GetOrderByIdQuery(id), ct);

    [HttpPost]
    public async Task<CreateOrderResponse> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateOrderCommand(request.Items), ct);
        return new CreateOrderResponse(id);
    }

    [HttpPut("{id:guid}")]
    public async Task<OperationResultDto> Update(Guid id, [FromBody] UpdateOrderRequest request, CancellationToken ct)
    {
        var ok = await _mediator.Send(new UpdateOrderCommand(id, request.Items, request.Status), ct);
        return new OperationResultDto(ok);
    }

    [HttpDelete("{id:guid}")]
    public async Task<OperationResultDto> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _mediator.Send(new DeleteOrderCommand(id), ct);
        return new OperationResultDto(ok);
    }
}
