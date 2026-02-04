using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Orders.Api.Controllers;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Models;
using Orders.Application.Orders.Queries;

namespace Orders.Tests.Controller;

public class OrdersControllerTest
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrdersController _controller;

    public OrdersControllerTest()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new OrdersController(_mediatorMock.Object);
    }

    #region List Tests

    [Fact]
    public async Task List_WithValidRequest_ReturnsOrdersList()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = new List<OrderReadDto>
        {
            new(Guid.NewGuid(), customerId, DateTime.UtcNow, 100m, 0, new List<OrderItemReadDto>()),
            new(Guid.NewGuid(), customerId, DateTime.UtcNow, 200m, 1, new List<OrderItemReadDto>())
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<OrderReadDto>)orders);

        // Act
        var result = await _controller.List(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _mediatorMock.Verify(m => m.Send(It.IsAny<ListOrdersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task List_WithEmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var emptyOrders = new List<OrderReadDto>();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ListOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<OrderReadDto>)emptyOrders);

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
            .Setup(m => m.Send(It.IsAny<ListOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _controller.List(cts.Token));
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_WithValidOrderId_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new OrderReadDto(orderId, customerId, DateTime.UtcNow, 100m, 0, new List<OrderItemReadDto>());

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.Get(orderId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetOrderByIdQuery>(q => q.Id == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithNonExistentOrderId_ReturnsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderReadDto?)null);

        // Act
        var result = await _controller.Get(orderId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Get_WithValidOrderId_SendsCorrectQuery()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new OrderReadDto(orderId, customerId, DateTime.UtcNow, 100m, 0, new List<OrderItemReadDto>());

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await _controller.Get(orderId, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetOrderByIdQuery>(q => q.Id == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsOrderId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest(new List<OrderItemRequest> { new(productId, 1) });
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithMultipleItems_SendsCorrectCommand()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var items = new List<OrderItemRequest>
        {
            new(productId1, 1),
            new(productId2, 2)
        };
        var request = new CreateOrderRequest(items);
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        await _controller.Create(request, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateOrderCommand>(c => c.Items.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsDynamicObjectWithId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest(new List<OrderItemRequest> { new(productId, 1) });
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new UpdateOrderRequest(
            new List<OrderItemRequest> { new(productId, 1) },
            1
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(orderId, request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ok);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithValidOrderId_SendsCorrectCommand()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new UpdateOrderRequest(
            new List<OrderItemRequest> { new(productId, 1) },
            0
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _controller.Update(orderId, request, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateOrderCommand>(c => c.Id == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidOrderId_ReturnsFalse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new UpdateOrderRequest(
            new List<OrderItemRequest> { new(productId, 1) },
            1
        );

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(orderId, request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Ok);
    }

    [Fact]
    public async Task Update_WithMultipleItems_SendsCorrectItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var items = new List<OrderItemRequest>
        {
            new(productId1, 2),
            new(productId2, 3)
        };
        var request = new UpdateOrderRequest(items, 2);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _controller.Update(orderId, request, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateOrderCommand>(c => c.Items.Count == 2 && c.Status == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidOrderId_ReturnsSuccess()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(orderId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ok);
        _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithValidOrderId_SendsCorrectCommand()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _controller.Delete(orderId, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteOrderCommand>(c => c.Id == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithInvalidOrderId_ReturnsFalse()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<DeleteOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(orderId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Ok);
    }

    #endregion
}
