namespace Storefront.Application;

using Storefront.Domain;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> ProcessMockPaymentAsync(Guid orderId, MockPaymentRequest request, CancellationToken cancellationToken = default);
}

