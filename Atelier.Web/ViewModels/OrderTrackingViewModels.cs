using System.ComponentModel.DataAnnotations;
using Atelier.Domain.Enums;

namespace Atelier.Web.ViewModels;

public sealed class OrderTrackingLookupViewModel
{
    [Required(ErrorMessage = "شماره سفارش را وارد کنید."), Display(Name = "شماره سفارش")] public string OrderNumber { get; set; } = string.Empty;
    [Required(ErrorMessage = "شماره موبایل را وارد کنید."), Display(Name = "شماره موبایل")] public string Phone { get; set; } = string.Empty;
}

public sealed class OrderTrackingViewModel
{
    public string Number { get; set; } = string.Empty;
    public string PublicToken { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public IReadOnlyList<OrderTrackingItemViewModel> Items { get; set; } = Array.Empty<OrderTrackingItemViewModel>();
    public IReadOnlyList<OrderTrackingStepViewModel> Timeline { get; set; } = Array.Empty<OrderTrackingStepViewModel>();
}

public sealed class OrderTrackingItemViewModel { public string Name { get; set; } = string.Empty; public int Quantity { get; set; } public decimal LineTotal { get; set; } }
public sealed class OrderTrackingStepViewModel { public OrderStatus Status { get; set; } public string Label { get; set; } = string.Empty; public string? Note { get; set; } public DateTime Date { get; set; } }
