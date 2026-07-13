using Atelier.Infrastructure.Data;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

[Route("order-tracking")]
public sealed class OrderTrackingController : PublicControllerBase
{
    public OrderTrackingController(AtelierDbContext db) : base(db) { }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Robots"] = "noindex,follow";
        SetSeoMetadata("پیگیری سفارش | mr2008.ir", "مشاهده امن وضعیت پرداخت، آماده‌سازی و ارسال سفارش.");
        return View(new OrderTrackingLookupViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("order-tracking")]
    public async Task<IActionResult> Lookup(OrderTrackingLookupViewModel model)
    {
        if (!ModelState.IsValid) return View("Index", model);
        var number = model.OrderNumber.Trim().ToUpperInvariant();
        var phone = NormalizePhone(model.Phone);
        var token = await DbContext.Orders.AsNoTracking().Where(x => x.Number == number && x.Phone == phone).Select(x => x.PublicToken).FirstOrDefaultAsync();
        if (token is null)
        {
            ModelState.AddModelError(string.Empty, "سفارشی با این مشخصات پیدا نشد. اطلاعات را دوباره بررسی کنید.");
            return View("Index", model);
        }
        return RedirectToAction(nameof(Details), new { token });
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> Details(string token)
    {
        var model = await DbContext.Orders.AsNoTracking().Where(x => x.PublicToken == token)
            .Select(x => new OrderTrackingViewModel
            {
                Number=x.Number, PublicToken=x.PublicToken, Status=x.Status, PaymentStatus=x.PaymentStatus, Total=x.Total, CreatedAt=x.CreatedAt,
                Destination=x.Province + "، " + x.City, Carrier=x.Carrier, TrackingNumber=x.TrackingNumber,
                Items=x.Items.Select(i => new OrderTrackingItemViewModel { Name=i.ProductName, Quantity=i.Quantity, LineTotal=i.LineTotal }).ToList(),
                Timeline=x.StatusHistory.OrderBy(h => h.CreatedAt).Select(h => new OrderTrackingStepViewModel { Status=h.Status, Label=h.Status.ToString(), Note=h.Note, Date=h.CreatedAt }).ToList()
            }).FirstOrDefaultAsync();
        if (model is null) return NotFound();
        foreach (var step in model.Timeline) step.Label = StatusLabel(step.Status);
        ViewData["Robots"] = "noindex,nofollow,noarchive";
        SetSeoMetadata("وضعیت سفارش | mr2008.ir", "جزئیات پیگیری امن سفارش.");
        return View(model);
    }

    private static string NormalizePhone(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0098")) return "0" + digits[4..];
        if (digits.StartsWith("98")) return "0" + digits[2..];
        return digits;
    }
    private static string StatusLabel(Atelier.Domain.Enums.OrderStatus status) => status switch
    {
        Atelier.Domain.Enums.OrderStatus.AwaitingPayment => "در انتظار پرداخت",
        Atelier.Domain.Enums.OrderStatus.AwaitingReview => "در انتظار بررسی",
        Atelier.Domain.Enums.OrderStatus.Confirmed => "تأیید شده",
        Atelier.Domain.Enums.OrderStatus.Preparing => "در حال آماده‌سازی",
        Atelier.Domain.Enums.OrderStatus.Shipped => "ارسال شده",
        Atelier.Domain.Enums.OrderStatus.Completed => "تحویل شده",
        _ => "لغو شده"
    };
}
