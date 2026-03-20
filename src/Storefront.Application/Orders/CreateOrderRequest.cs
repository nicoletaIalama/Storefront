namespace Storefront.Application;

public sealed class CreateOrderRequest
{
    public IReadOnlyList<CreateOrderLineItemRequest> LineItems { get; init; } = Array.Empty<CreateOrderLineItemRequest>();
}

public sealed class CreateOrderLineItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

