using Microsoft.Extensions.Logging;
using Moq;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Commands;
using Orders.Application.Orders.Handlers;
using Orders.Application.Orders.Models;
using Orders.Domain.Orders;
using Orders.Domain.Products;

namespace Orders.Tests.Handlers;

public class CreateOrderCommandHandlerTest
{
    private readonly Mock<IProductRepository> _productsMock;
    private readonly Mock<IOrderWriteRepository> _ordersMock;
    private readonly Mock<IFixedCustomerProvider> _fixedCustomerMock;
    private readonly Mock<IOrderProjectionWriter> _projectionMock;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTest()
    {
        _productsMock = new Mock<IProductRepository>();
        _ordersMock = new Mock<IOrderWriteRepository>();
        _fixedCustomerMock = new Mock<IFixedCustomerProvider>();
        _projectionMock = new Mock<IOrderProjectionWriter>();
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CreateOrderCommandHandler>.Instance;
        
        _handler = new CreateOrderCommandHandler(
            _productsMock.Object,
            _ordersMock.Object,
            _fixedCustomerMock.Object,
            _projectionMock.Object,
            _logger
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesOrderAndReturnsId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Test Product", 100m);
        var command = new CreateOrderCommand(
            new List<OrderItemRequest> { new(productId, 1) }
        );

        _fixedCustomerMock.Setup(x => x.GetCustomerId()).Returns(customerId);
        _productsMock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _ordersMock.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _ordersMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _projectionMock.Setup(x => x.UpsertAsync(It.IsAny<OrderReadDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _ordersMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _ordersMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ThrowsInvalidOperationException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var nonExistentProductId = Guid.NewGuid();
        var command = new CreateOrderCommand(
            new List<OrderItemRequest> { new(nonExistentProductId, 1) }
        );

        _fixedCustomerMock.Setup(x => x.GetCustomerId()).Returns(customerId);
        _productsMock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_WhenProjectionFails_DoesNotThrow()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Test Product", 100m);
        var command = new CreateOrderCommand(
            new List<OrderItemRequest> { new(productId, 1) }
        );

        _fixedCustomerMock.Setup(x => x.GetCustomerId()).Returns(customerId);
        _productsMock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _ordersMock.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _ordersMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _projectionMock.Setup(x => x.UpsertAsync(It.IsAny<OrderReadDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Projection failed"));

        // Act & Assert - Should not throw
        var result = await _handler.Handle(command, CancellationToken.None);
        Assert.NotEqual(Guid.Empty, result);
    }
}
