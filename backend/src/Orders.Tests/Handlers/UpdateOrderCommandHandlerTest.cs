using Microsoft.Extensions.Logging;
using Moq;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Handlers;
using Orders.Application.Orders.Models;
using Orders.Domain.Orders;
using Orders.Domain.Products;

namespace Orders.Tests.Handlers;

public class UpdateOrderCommandHandlerTest
{
    private readonly Mock<IProductRepository> _productsMock;
    private readonly Mock<IOrderWriteRepository> _ordersMock;
    private readonly Mock<IOrderProjectionWriter> _projectionMock;
    private readonly ILogger<UpdateOrderCommandHandler> _logger;
    private readonly UpdateOrderCommandHandler _handler;

    public UpdateOrderCommandHandlerTest()
    {
        _productsMock = new Mock<IProductRepository>();
        _ordersMock = new Mock<IOrderWriteRepository>();
        _projectionMock = new Mock<IOrderProjectionWriter>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<UpdateOrderCommandHandler>.Instance;
        
        _handler = new UpdateOrderCommandHandler(
            _productsMock.Object,
            _ordersMock.Object,
            _projectionMock.Object,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesOrderAndReturnsTrue()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Updated Product", 150m);
        
        var order = new Order(orderId, customerId, DateTime.UtcNow);
        order.AddOrUpdateItem(new Product(productId, "Old Product", 100m), 1);

        var command = new UpdateOrderCommand(
            orderId,
            new List<OrderItemRequest> { new(productId, 2) },
            (int)OrderStatus.Paid
        );

        _ordersMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _productsMock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _ordersMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _projectionMock.Setup(x => x.UpsertAsync(It.IsAny<OrderReadDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _ordersMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ReturnsFalse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var command = new UpdateOrderCommand(
            orderId,
            new List<OrderItemRequest> { new(productId, 1) },
            0
        );

        _ordersMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_WhenProjectionFails_DoesNotThrow()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Updated Product", 150m);
        
        var order = new Order(orderId, customerId, DateTime.UtcNow);
        order.AddOrUpdateItem(new Product(productId, "Old Product", 100m), 1);

        var command = new UpdateOrderCommand(
            orderId,
            new List<OrderItemRequest> { new(productId, 2) },
            (int)OrderStatus.Paid
        );

        _ordersMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _productsMock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _ordersMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _projectionMock.Setup(x => x.UpsertAsync(It.IsAny<OrderReadDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Projection failed"));

        // Act & Assert - Should not throw
        var result = await _handler.Handle(command, CancellationToken.None);
        Assert.True(result);
    }
}
