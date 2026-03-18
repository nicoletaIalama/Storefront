namespace Storefront.Application;

public sealed class MockPaymentDto
{
    public bool Succeeded { get; init; }
    public string Provider { get; init; } = "Mock";
    public string? FailureReason { get; init; }
}

