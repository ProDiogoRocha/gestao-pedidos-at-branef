using Microsoft.Extensions.Logging;
using Moq;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Handlers;
using Orders.Domain.Orders;

namespace Orders.Tests.Handlers;

public class DeleteOrderCommandHandlerTest
{
    private readonly Mock<IOrderWriteRepository> _ordersMock;
    private readonly Mock<IOrderProjectionWriter> _projectionMock;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTest()
    {
        _ordersMock = new Mock<IOrderWriteRepository>();
        _projectionMock = new Mock<IOrderProjectionWriter>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DeleteOrderCommandHandler>.Instance;
        
        _handler = new DeleteOrderCommandHandler(
            _ordersMock.Object,
            _projectionMock.Object,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidOrder_DeletesAndReturnsTrue()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new Order(orderId, customerId, DateTime.UtcNow);
        var command = new DeleteOrderCommand(orderId);

        _ordersMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _ordersMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _projectionMock.Setup(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _ordersMock.Verify(x => x.Remove(order), Times.Once);
        _ordersMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ReturnsFalse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        _ordersMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _ordersMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProjectionFails_DoesNotThrow()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new Order(orderId, customerId, DateTime.UtcNow);
        var command = new DeleteOrderCommand(orderId);

        _ordersMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _ordersMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _projectionMock.Setup(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Projection failed"));

        // Act & Assert - Should not throw
        var result = await _handler.Handle(command, CancellationToken.None);
        Assert.True(result);
    }
}
