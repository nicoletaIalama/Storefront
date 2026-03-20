using System.Text.Json.Serialization;

namespace Storefront.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    PendingPayment,
    Paid,
    PaymentFailed
}

