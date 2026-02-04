using Moq;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Handlers;
using Orders.Domain.Products;

namespace Orders.Tests.Handlers;

public class QueryHandlersTest
{
    [Fact]
    public async Task GetOrderById_ReturnsDto()
    {
        var readMock = new Mock<IOrderReadRepository>();
        var dto = new Orders.Application.Orders.Models.OrderReadDto(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, 10m, 0, new List<Orders.Application.Orders.Models.OrderItemReadDto>());

        readMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var handler = new GetOrderByIdQueryHandler(readMock.Object);

        var result = await handler.Handle(new Orders.Application.Orders.Queries.GetOrderByIdQuery(dto.Id), CancellationToken.None);

        Assert.Equal(dto, result);
    }

    [Fact]
    public async Task GetOrderById_ReturnsNull()
    {
        var readMock = new Mock<IOrderReadRepository>();
        readMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Orders.Application.Orders.Models.OrderReadDto?)null);

        var handler = new GetOrderByIdQueryHandler(readMock.Object);

        var result = await handler.Handle(new Orders.Application.Orders.Queries.GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ListOrders_ReturnsList()
    {
        var readMock = new Mock<IOrderReadRepository>();
        var list = new List<Orders.Application.Orders.Models.OrderReadDto>();
        readMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var handler = new ListOrdersQueryHandler(readMock.Object);

        var result = await handler.Handle(new Orders.Application.Orders.Queries.ListOrdersQuery(), CancellationToken.None);

        Assert.Same(list, result);
    }

    [Fact]
    public async Task ListProducts_MapsProducts()
    {
        var productsMock = new Mock<IProductRepository>();
        var p1 = new Product(Guid.NewGuid(), "P1", 1m);
        var p2 = new Product(Guid.NewGuid(), "P2", 2m);
        productsMock.Setup(p => p.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Product> { p1, p2 });

        var handler = new ListProductsQueryHandler(productsMock.Object);

        var result = await handler.Handle(new Orders.Application.Orders.Queries.ListProductsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == p1.Id && r.Name == p1.Name && r.Price == p1.Price);
        Assert.Contains(result, r => r.Id == p2.Id && r.Name == p2.Name && r.Price == p2.Price);
    }
}
