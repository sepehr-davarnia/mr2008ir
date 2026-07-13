using Atelier.Domain.Enums;

public class Order : Entity
{
    private readonly List<OrderItem> _items = new();
    private readonly List<OrderStatusHistory> _statusHistory = new();

    public string Number { get; private set; }
    public string PublicToken { get; private set; }
    public string CustomerName { get; private set; }
    public string Phone { get; private set; }
    public string Province { get; private set; }
    public string City { get; private set; }
    public string Address { get; private set; }
    public string? PostalCode { get; private set; }
    public string? CustomerNote { get; private set; }
    public string? Carrier { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    protected Order()
    {
        Number = "N/A";
        PublicToken = "N/A";
        CustomerName = "N/A";
        Phone = "N/A";
        Province = "N/A";
        City = "N/A";
        Address = "N/A";
    }

    public Order(string number, string publicToken, string customerName, string phone, string province, string city,
        string address, string? postalCode, string? customerNote)
    {
        Number = number;
        PublicToken = publicToken;
        CustomerName = customerName;
        Phone = phone;
        Province = province;
        City = city;
        Address = address;
        PostalCode = postalCode;
        CustomerNote = customerNote;
        Status = OrderStatus.AwaitingPayment;
        PaymentStatus = PaymentStatus.Pending;
        _statusHistory.Add(new OrderStatusHistory(Status, "سفارش ایجاد شد و در انتظار پرداخت است."));
    }

    public void AddItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        if (unitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));
        if (quantity is < 1 or > 20) throw new ArgumentOutOfRangeException(nameof(quantity));

        _items.Add(new OrderItem(productId, productName, unitPrice, quantity));
        Total = _items.Sum(item => item.LineTotal);
        MarkUpdated();
    }

    public void UpdateStatus(OrderStatus status)
    {
        if (Status == status) return;
        Status = status;
        _statusHistory.Add(new OrderStatusHistory(status));
        MarkUpdated();
    }

    public void MarkPaid()
    {
        PaymentStatus = PaymentStatus.Paid;
        UpdateStatus(OrderStatus.AwaitingReview);
    }

    public void MarkPaymentFailed(bool cancelled = false)
    {
        PaymentStatus = cancelled ? PaymentStatus.Cancelled : PaymentStatus.Failed;
        MarkUpdated();
    }

    public void MarkPaymentPending()
    {
        if (PaymentStatus == PaymentStatus.Paid) return;
        PaymentStatus = PaymentStatus.Pending;
        MarkUpdated();
    }

    public void SetShipment(string? carrier, string? trackingNumber)
    {
        Carrier = string.IsNullOrWhiteSpace(carrier) ? null : carrier.Trim();
        TrackingNumber = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber.Trim();
        ShippedAt ??= DateTime.UtcNow;
        MarkUpdated();
    }
}
