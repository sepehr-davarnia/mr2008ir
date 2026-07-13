using Atelier.Domain.Enums;

public class OrderStatusHistory : Entity
{
    public int OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Note { get; private set; }

    protected OrderStatusHistory() { }

    public OrderStatusHistory(OrderStatus status, string? note = null)
    {
        Status = status;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}
