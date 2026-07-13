using Atelier.Infrastructure.Data;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

[Route("checkout")]
public class CheckoutController : PublicControllerBase
{
    private readonly ICartService _cart;
    private readonly ICartViewModelBuilder _cartBuilder;
    private readonly IPaymentGateway _paymentGateway;

    public CheckoutController(AtelierDbContext dbContext, ICartService cart, ICartViewModelBuilder cartBuilder, IPaymentGateway paymentGateway) : base(dbContext)
    {
        _cart = cart;
        _cartBuilder = cartBuilder;
        _paymentGateway = paymentGateway;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _cartBuilder.BuildAsync();
        if (!cart.Items.Any()) return RedirectToAction("Index", "Cart");

        ViewData["Robots"] = "noindex,nofollow";
        SetSeoMetadata("تکمیل سفارش | mr2008.ir", "ثبت امن مشخصات ارسال سفارش.");
        return View(new CheckoutViewModel { Cart = cart });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("checkout")]
    public async Task<IActionResult> Submit(CheckoutViewModel model)
    {
        model.Cart = await _cartBuilder.BuildAsync();
        if (!model.Cart.Items.Any()) ModelState.AddModelError(string.Empty, "سبد خرید خالی است یا قیمت محصول تغییر کرده است.");
        if (!ModelState.IsValid)
        {
            ViewData["Robots"] = "noindex,nofollow";
            SetSeoMetadata("تکمیل سفارش | mr2008.ir", "ثبت امن مشخصات ارسال سفارش.");
            return View("Index", model);
        }

        var number = $"MR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
        var publicToken = Guid.NewGuid().ToString("N");
        var order = new Order(number, publicToken, model.CustomerName.Trim(), NormalizePhone(model.Phone), model.Province.Trim(),
            model.City.Trim(), model.Address.Trim(), EmptyToNull(model.PostalCode), EmptyToNull(model.CustomerNote));
        foreach (var item in model.Cart.Items)
            order.AddItem(item.ProductId, item.Name, item.UnitPrice, item.Quantity);

        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();
        var transaction = new PaymentTransaction(order.Id, "Zarinpal", order.Total);
        DbContext.PaymentTransactions.Add(transaction);
        await DbContext.SaveChangesAsync();

        var callbackUrl = Url.Action("Callback", "Payment", new { token = publicToken }, Request.Scheme)!;
        var payment = await _paymentGateway.RequestAsync(order.Total, $"پرداخت سفارش {order.Number}", callbackUrl, order.Phone, order.Number, HttpContext.RequestAborted);
        if (!payment.Succeeded || string.IsNullOrWhiteSpace(payment.Authority) || string.IsNullOrWhiteSpace(payment.RedirectUrl))
        {
            transaction.MarkFailed(payment.Code, payment.Error);
            order.MarkPaymentFailed();
            await DbContext.SaveChangesAsync();
            return RedirectToAction("Result", "Payment", new { token = publicToken });
        }

        transaction.MarkRequested(payment.Authority, payment.Code);
        await DbContext.SaveChangesAsync();
        _cart.Clear();

        return Redirect(payment.RedirectUrl);
    }

    [HttpGet("confirmation/{token}")]
    public async Task<IActionResult> Confirmation(string token)
    {
        var order = await DbContext.Orders.AsNoTracking().Where(item => item.PublicToken == token && item.PaymentStatus == Atelier.Domain.Enums.PaymentStatus.Paid)
            .Select(item => new OrderConfirmationViewModel { OrderNumber = item.Number, PublicToken = item.PublicToken, Total = item.Total, Phone = item.Phone })
            .FirstOrDefaultAsync();
        if (order is null) return NotFound();

        ViewData["Robots"] = "noindex,nofollow";
        SetSeoMetadata("سفارش ثبت شد | mr2008.ir", "تأیید ثبت سفارش.");
        return View(order);
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0098")) digits = "0" + digits[4..];
        else if (digits.StartsWith("98")) digits = "0" + digits[2..];
        return digits;
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
