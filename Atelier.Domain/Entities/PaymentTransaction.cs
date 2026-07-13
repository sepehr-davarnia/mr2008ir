using Atelier.Domain.Enums;

public class PaymentTransaction : Entity
{
    public int OrderId { get; private set; }
    public string Gateway { get; private set; }
    public string? Authority { get; private set; }
    public string? ReferenceId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public int? GatewayCode { get; private set; }
    public string? FailureReason { get; private set; }

    protected PaymentTransaction() { Gateway = "N/A"; }

    public PaymentTransaction(int orderId, string gateway, decimal amount)
    {
        OrderId = orderId;
        Gateway = gateway;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    public void MarkRequested(string authority, int gatewayCode)
    {
        Authority = authority;
        GatewayCode = gatewayCode;
        MarkUpdated();
    }

    public void MarkPaid(string referenceId, int gatewayCode)
    {
        ReferenceId = referenceId;
        GatewayCode = gatewayCode;
        Status = PaymentStatus.Paid;
        FailureReason = null;
        MarkUpdated();
    }

    public void MarkFailed(int? gatewayCode, string? reason, bool cancelled = false)
    {
        GatewayCode = gatewayCode;
        FailureReason = reason;
        Status = cancelled ? PaymentStatus.Cancelled : PaymentStatus.Failed;
        MarkUpdated();
    }
}
