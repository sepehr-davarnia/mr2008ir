namespace Atelier.Domain.Enums;

public enum OrderStatus
{
    AwaitingPayment = 0,
    AwaitingReview = 1,
    Confirmed = 2,
    Preparing = 3,
    Shipped = 4,
    Completed = 5,
    Cancelled = 6
}
