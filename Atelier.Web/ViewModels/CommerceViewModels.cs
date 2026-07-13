using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.ViewModels;

public class CartViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = Array.Empty<CartItemViewModel>();
    public decimal Total { get; set; }
    public int TotalQuantity => Items.Sum(item => item.Quantity);
}

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();

    [Required(ErrorMessage = "نام و نام خانوادگی را وارد کنید.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "نام باید بین ۳ تا ۱۲۰ کاراکتر باشد.")]
    [Display(Name = "نام و نام خانوادگی")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل را وارد کنید.")]
    [RegularExpression(@"^(?:\+98|0098|98|0)?9\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست.")]
    [Display(Name = "شماره موبایل")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "استان را وارد کنید.")]
    [StringLength(80)]
    [Display(Name = "استان")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "شهر را وارد کنید.")]
    [StringLength(80)]
    [Display(Name = "شهر")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "نشانی کامل را وارد کنید.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "نشانی کامل‌تری وارد کنید.")]
    [Display(Name = "نشانی کامل")]
    public string Address { get; set; } = string.Empty;

    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد پستی باید ۱۰ رقم باشد.")]
    [Display(Name = "کد پستی")]
    public string? PostalCode { get; set; }

    [StringLength(1000)]
    [Display(Name = "توضیحات سفارش")]
    public string? CustomerNote { get; set; }
}

public class OrderConfirmationViewModel
{
    public string OrderNumber { get; set; } = string.Empty;
    public string PublicToken { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Phone { get; set; } = string.Empty;
}

public sealed class PaymentResultViewModel
{
    public bool Succeeded { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string PublicToken { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string Message { get; set; } = string.Empty;
}
