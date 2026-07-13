namespace Atelier.Domain.Enums;

public enum OrderStatus
{
    AwaitingReview = 0,
    Confirmed = 1,
    Preparing = 2,
    Shipped = 3,
    Completed = 4,
    Cancelled = 5
}
