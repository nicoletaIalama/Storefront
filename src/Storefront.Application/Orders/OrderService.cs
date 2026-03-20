using Storefront.Domain;

namespace Storefront.Application;

public sealed class OrderService(IOrderRepository orderRepository, IProductService productService) : IOrderService
{
    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.LineItems is null || request.LineItems.Count == 0)
        {
            throw new ArgumentException("At least one line item is required.", nameof(request));
        }

        var lineItems = new List<OrderLineItem>();
        decimal total = 0m;

        foreach (var item in request.LineItems)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0.", nameof(request));
            }

            var product = await productService.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                throw new ArgumentException($"Unknown productId: {item.ProductId}", nameof(request));
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException($"Product is not active: {product.Name}");
            }

            lineItems.Add(new OrderLineItem { ProductId = item.ProductId, Quantity = item.Quantity });
            total += product.Price * item.Quantity;
        }

        var order = new Order(Guid.NewGuid(), lineItems, total);
        await orderRepository.AddAsync(order, cancellationToken);

        return MapToDto(order);
    }

    public async Task<OrderDto> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            throw new KeyNotFoundException($"Order not found: {orderId}");
        }

        return MapToDto(order);
    }

    public async Task<OrderDto> ProcessMockPaymentAsync(
        Guid orderId,
        MockPaymentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Result))
        {
            throw new ArgumentException("Result is required.", nameof(request));
        }

        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);
        if (order is null)
        {
            throw new KeyNotFoundException($"Order not found: {orderId}");
        }

        if (order.Status != OrderStatus.PendingPayment)
        {
            throw new InvalidOperationException($"Cannot process payment when order is '{order.Status}'.");
        }

        var result = request.Result.Trim();
        if (result.Equals("success", StringComparison.OrdinalIgnoreCase))
        {
            order.MarkPaid();
        }
        else if (result.Equals("failure", StringComparison.OrdinalIgnoreCase))
        {
            order.MarkPaymentFailed(request.FailureReason ?? "Payment failed.");
        }
        else
        {
            throw new ArgumentException("Result must be 'success' or 'failure'.", nameof(request));
        }

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.Id,
            Total = order.Total,
            Status = order.Status,
            LineItems = order.LineItems
                .Select(li => new OrderLineItemDto { ProductId = li.ProductId, Quantity = li.Quantity })
                .ToArray(),
            Payment = order.Payment is null
                ? null
                : new MockPaymentDto
                {
                    Succeeded = order.Payment.Succeeded,
                    Provider = order.Payment.Provider,
                    FailureReason = order.Payment.FailureReason
                }
        };
    }
}

