
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Orders.Models;
using Orders.Application.Orders.Queries;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IReadOnlyList<ProductDto>> List(CancellationToken ct)
        => await _mediator.Send(new ListProductsQuery(), ct);
}
