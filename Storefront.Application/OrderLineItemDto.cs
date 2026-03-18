namespace Storefront.Application;

public sealed class OrderLineItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

