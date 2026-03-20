namespace Storefront.Domain;

public sealed class PaymentResult
{
    public bool Succeeded { get; init; }
    public string Provider { get; init; } = "Mock";
    public string? FailureReason { get; init; }
}

