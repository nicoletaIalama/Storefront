using Moq;
using Storefront.Domain;
using Xunit;

namespace Storefront.Application.Tests;

public sealed class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IProductService> _productService = new();

    private OrderService CreateSut() => new(_orderRepository.Object, _productService.Object);

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Task Act() => sut.CreateAsync(null!);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(Act);
    }

    [Fact]
    public async Task CreateAsync_EmptyLineItems_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        var request = new CreateOrderRequest { LineItems = Array.Empty<CreateOrderLineItemRequest>() };

        // Act
        Task Act() => sut.CreateAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        Assert.Equal("request", ex.ParamName);
        Assert.Contains("line item", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_NullLineItems_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        var request = new CreateOrderRequest { LineItems = null! };

        // Act
        Task Act() => sut.CreateAsync(request);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }

    [Fact]
    public async Task CreateAsync_ZeroQuantity_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest
        {
            LineItems = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 0 }]
        };

        // Act
        Task Act() => sut.CreateAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task CreateAsync_UnknownProduct_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        var productId = Guid.NewGuid();
        _productService
            .Setup(p => p.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);

        var request = new CreateOrderRequest
        {
            LineItems = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1 }]
        };

        // Act
        Task Act() => sut.CreateAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        Assert.Contains(productId.ToString(), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAsync_InactiveProduct_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = CreateSut();
        var productId = Guid.NewGuid();
        _productService
            .Setup(p => p.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ProductDto
                {
                    Id = productId,
                    Name = "Delisted",
                    Price = 5m,
                    Description = null,
                    IsActive = false
                });

        var request = new CreateOrderRequest
        {
            LineItems = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1 }]
        };

        // Act
        Task Act() => sut.CreateAsync(request);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(Act);
        Assert.Contains("Delisted", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsOrderWithCorrectTotal()
    {
        // Arrange
        var sut = CreateSut();
        var productId = Guid.NewGuid();
        _productService
            .Setup(p => p.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ProductDto
                {
                    Id = productId,
                    Name = "Widget",
                    Price = 12.5m,
                    Description = "x",
                    IsActive = true
                });

        var request = new CreateOrderRequest
        {
            LineItems = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 2 }]
        };

        // Act
        var dto = await sut.CreateAsync(request);

        // Assert
        Assert.Equal(25m, dto.Total);
        Assert.Equal(OrderStatus.PendingPayment, dto.Status);
        Assert.Single(dto.LineItems);
        Assert.Equal(productId, dto.LineItems[0].ProductId);
        Assert.Equal(2, dto.LineItems[0].Quantity);

        _orderRepository.Verify(
            r => r.AddAsync(It.Is<Order>(o => o.Total == 25m && o.LineItems.Count == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        _orderRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Task Act() => sut.GetByIdAsync(id);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(Act);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsMappedDto()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        var lineItems = new[] { new OrderLineItem { ProductId = Guid.NewGuid(), Quantity = 3 } };
        var order = new Order(id, lineItems, 99m);
        _orderRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var dto = await sut.GetByIdAsync(id);

        // Assert
        Assert.Equal(id, dto.OrderId);
        Assert.Equal(99m, dto.Total);
        Assert.Equal(OrderStatus.PendingPayment, dto.Status);
        Assert.Single(dto.LineItems);
        Assert.Equal(3, dto.LineItems[0].Quantity);
    }

    [Fact]
    public async Task ProcessMockPaymentAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Task Act() => sut.ProcessMockPaymentAsync(Guid.NewGuid(), null!);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(Act);
    }

    [Fact]
    public async Task ProcessMockPaymentAsync_EmptyResult_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        Task Act() => sut.ProcessMockPaymentAsync(Guid.NewGuid(), new MockPaymentRequest { Result = "   " });

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(Act);
    }

    [Fact]
    public async Task ProcessMockPaymentAsync_OrderNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        _orderRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Task Act() => sut.ProcessMockPaymentAsync(id, new MockPaymentRequest { Result = "success" });

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(Act);
    }

    [Fact]
    public async Task ProcessMockPaymentAsync_WrongStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        var lineItems = new[] { new OrderLineItem { ProductId = Guid.NewGuid(), Quantity = 1 } };
        var order = new Order(id, lineItems, 10m);
        order.MarkPaid();
        _orderRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        Task Act() => sut.ProcessMockPaymentAsync(id, new MockPaymentRequest { Result = "success" });

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }

    [Fact]
    public async Task ProcessMockPaymentAsync_Success_MarksPaid()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        var lineItems = new[] { new OrderLineItem { ProductId = Guid.NewGuid(), Quantity = 1 } };
        var order = new Order(id, lineItems, 10m);
        _orderRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var dto = await sut.ProcessMockPaymentAsync(id, new MockPaymentRequest { Result = "SUCCESS" });

        // Assert
        Assert.Equal(OrderStatus.Paid, dto.Status);
        Assert.NotNull(dto.Payment);
        Assert.True(dto.Payment!.Succeeded);
    }

    [Fact]
    public async Task ProcessMockPaymentAsync_Failure_MarksPaymentFailed()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        var lineItems = new[] { new OrderLineItem { ProductId = Guid.NewGuid(), Quantity = 1 } };
        var order = new Order(id, lineItems, 10m);
        _orderRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var dto = await sut.ProcessMockPaymentAsync(
            id,
            new MockPaymentRequest { Result = "Failure", FailureReason = "Card declined" });

        // Assert
        Assert.Equal(OrderStatus.PaymentFailed, dto.Status);
        Assert.NotNull(dto.Payment);
        Assert.False(dto.Payment!.Succeeded);
        Assert.Equal("Card declined", dto.Payment.FailureReason);
    }

    [Fact]
    public async Task ProcessMockPaymentAsync_InvalidResult_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        var id = Guid.NewGuid();
        var lineItems = new[] { new OrderLineItem { ProductId = Guid.NewGuid(), Quantity = 1 } };
        var order = new Order(id, lineItems, 10m);
        _orderRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        Task Act() => sut.ProcessMockPaymentAsync(id, new MockPaymentRequest { Result = "maybe" });

        // Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(Act);
        Assert.Equal("request", ex.ParamName);
    }
}
