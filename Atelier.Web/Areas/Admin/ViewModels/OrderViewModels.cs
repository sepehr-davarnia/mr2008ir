using Atelier.Domain.Enums;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class OrderListItemViewModel
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int ItemsCount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderDetailsViewModel
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? CustomerNote { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<OrderItemViewModel> Items { get; set; } = Array.Empty<OrderItemViewModel>();
    public IReadOnlyList<OrderHistoryItemViewModel> History { get; set; } = Array.Empty<OrderHistoryItemViewModel>();
    public IReadOnlyList<PaymentItemViewModel> Payments { get; set; } = Array.Empty<PaymentItemViewModel>();
}

public class OrderHistoryItemViewModel { public OrderStatus Status { get; set; } public string? Note { get; set; } public DateTime CreatedAt { get; set; } }
public class PaymentItemViewModel { public string Gateway { get; set; } = string.Empty; public PaymentStatus Status { get; set; } public string? ReferenceId { get; set; } public int? GatewayCode { get; set; } public DateTime CreatedAt { get; set; } }

public class OrderItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
