using Atelier.Domain.Enums;

public class Order : Entity
{
    private readonly List<OrderItem> _items = new();

    public string Number { get; private set; }
    public string CustomerName { get; private set; }
    public string Phone { get; private set; }
    public string Province { get; private set; }
    public string City { get; private set; }
    public string Address { get; private set; }
    public string? PostalCode { get; private set; }
    public string? CustomerNote { get; private set; }
    public decimal Total { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    protected Order()
    {
        Number = "N/A";
        CustomerName = "N/A";
        Phone = "N/A";
        Province = "N/A";
        City = "N/A";
        Address = "N/A";
    }

    public Order(string number, string customerName, string phone, string province, string city,
        string address, string? postalCode, string? customerNote)
    {
        Number = number;
        CustomerName = customerName;
        Phone = phone;
        Province = province;
        City = city;
        Address = address;
        PostalCode = postalCode;
        CustomerNote = customerNote;
        Status = OrderStatus.AwaitingReview;
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
        Status = status;
        MarkUpdated();
    }
}
