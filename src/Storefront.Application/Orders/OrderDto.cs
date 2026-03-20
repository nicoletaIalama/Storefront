using System.Text.Json.Serialization;
using Storefront.Domain;

namespace Storefront.Application;

public sealed class OrderDto
{
    public Guid OrderId { get; init; }
    public IReadOnlyList<OrderLineItemDto> LineItems { get; init; } = Array.Empty<OrderLineItemDto>();
    public decimal Total { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus Status { get; init; }

    public MockPaymentDto? Payment { get; init; }
}

