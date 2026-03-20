using System.Collections.Generic;
using System.Linq;

namespace Storefront.Domain;

public sealed class Order
{
    private readonly List<OrderLineItem> _lineItems;

    public Guid Id { get; init; }
    public decimal Total { get; init; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLineItem> LineItems => _lineItems;

    public PaymentResult? Payment { get; private set; }

    public Order(Guid id, IReadOnlyList<OrderLineItem> lineItems, decimal total)
    {
        Id = id;
        _lineItems = lineItems.ToList();
        Total = total;
        Status = OrderStatus.PendingPayment;
    }

    public void MarkPaid()
    {
        Status = OrderStatus.Paid;
        Payment = new PaymentResult { Succeeded = true, Provider = "Mock" };
    }

    public void MarkPaymentFailed(string? failureReason)
    {
        Status = OrderStatus.PaymentFailed;
        Payment = new PaymentResult { Succeeded = false, Provider = "Mock", FailureReason = failureReason };
    }
}

