namespace Storefront.Domain;

public sealed class OrderLineItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

