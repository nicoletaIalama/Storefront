namespace Storefront.Application;

public sealed class MockPaymentRequest
{
    // Accepted values (case-insensitive): "success" or "failure".
    public string Result { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
}

