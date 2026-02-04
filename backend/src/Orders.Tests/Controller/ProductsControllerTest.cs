using MediatR;
using Moq;
using Orders.Api.Controllers;
using Orders.Application.Orders.Models;
using Orders.Application.Orders.Queries;

namespace Orders.Tests.Controller;

public class ProductsControllerTest
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsController _controller;

    public ProductsControllerTest()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsController(_mediatorMock.Object);
    }

    #region List Tests

    [Fact]
    public async Task List_WithValidRequest_ReturnsProductsList()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new(Guid.NewGuid(), "Product 1", 100.00m),
            new(Guid.NewGuid(), "Product 2", 200.00m),
            new(Guid.NewGuid(), "Product 3", 300.00m)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ProductDto>)products);

        // Act
        var result = await _controller.List(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        _mediatorMock.Verify(m => m.Send(It.IsAny<ListProductsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task List_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var emptyProducts = new List<ProductDto>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ProductDto>)emptyProducts);

        // Act
        var result = await _controller.List(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task List_WithCancellation_CancellationTokenIsPropagated()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListProductsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _controller.List(cts.Token));
    }

    [Fact]
    public async Task List_SendsListProductsQuery()
    {
        // Arrange
        var products = new List<ProductDto>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ProductDto>)products);

        // Act
        await _controller.List(CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<ListProductsQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task List_WithMultipleProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<ProductDto>();
        for (int i = 1; i <= 10; i++)
        {
            products.Add(new ProductDto(Guid.NewGuid(), $"Product {i}", i * 100m));
        }

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ProductDto>)products);

        // Act
        var result = await _controller.List(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task List_WithValidProducts_ReturnsProductsWithCorrectProperties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var products = new List<ProductDto>
        {
            new ProductDto(productId, "Test Product", 150.50m)
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<ProductDto>)products);

        // Act
        var result = await _controller.List(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var product = result.First();
        Assert.Equal(productId, product.Id);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(150.50m, product.Price);
    }

    [Fact]
    public async Task List_MediatorIsCalled_WithCorrectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var products = new List<ProductDto>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListProductsQuery>(), cts.Token))
            .ReturnsAsync((IReadOnlyList<ProductDto>)products);

        // Act
        await _controller.List(cts.Token);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<ListProductsQuery>(),
            cts.Token), Times.Once);
    }

    #endregion
}
