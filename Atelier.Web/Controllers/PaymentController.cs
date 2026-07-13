using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace Atelier.Web.Controllers;

[Route("payment/zarinpal")]
public sealed class PaymentController : PublicControllerBase
{
    private readonly IPaymentGateway _gateway;
    public PaymentController(AtelierDbContext db, IPaymentGateway gateway) : base(db) => _gateway = gateway;

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string token, string? Status, string? Authority)
    {
        var order = await DbContext.Orders.FirstOrDefaultAsync(x => x.PublicToken == token);
        if (order is null) return NotFound();
        var transaction = string.IsNullOrWhiteSpace(Authority)
            ? await DbContext.PaymentTransactions.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.OrderId == order.Id)
            : await DbContext.PaymentTransactions.FirstOrDefaultAsync(x => x.OrderId == order.Id && x.Authority == Authority);
        if (transaction is null) return NotFound();
        if (order.PaymentStatus == PaymentStatus.Paid) return RedirectToAction("Confirmation", "Checkout", new { token });

        if (!string.Equals(Status, "OK", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(Authority))
        {
            transaction.MarkFailed(null, "پرداخت توسط کاربر لغو شد یا پاسخ درگاه معتبر نبود.", true);
            order.MarkPaymentFailed(true);
            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Result), new { token });
        }

        var verified = await _gateway.VerifyAsync(order.Total, Authority, HttpContext.RequestAborted);
        if (verified.Succeeded)
        {
            transaction.MarkPaid(verified.ReferenceId ?? Authority, verified.Code);
            order.MarkPaid();
            await DbContext.SaveChangesAsync();
            return RedirectToAction("Confirmation", "Checkout", new { token });
        }

        transaction.MarkFailed(verified.Code, verified.Error);
        order.MarkPaymentFailed();
        await DbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Result), new { token });
    }

    [HttpGet("result/{token}")]
    public async Task<IActionResult> Result(string token)
    {
        var model = await DbContext.Orders.AsNoTracking().Where(x => x.PublicToken == token)
            .Select(x => new PaymentResultViewModel { Succeeded = x.PaymentStatus == PaymentStatus.Paid, OrderNumber = x.Number, PublicToken = x.PublicToken, Message = x.PaymentStatus == PaymentStatus.Cancelled ? "پرداخت لغو شد؛ سفارش شما محفوظ است." : "پرداخت تأیید نشد. برای راهنمایی با فروشگاه تماس بگیرید." }).FirstOrDefaultAsync();
        if (model is null) return NotFound();
        ViewData["Robots"] = "noindex,nofollow";
        SetSeoMetadata("نتیجه پرداخت | mr2008.ir", "نتیجه امن پرداخت سفارش.");
        return View(model);
    }

    [HttpPost("retry/{token}"), ValidateAntiForgeryToken, EnableRateLimiting("checkout")]
    public async Task<IActionResult> Retry(string token)
    {
        var order = await DbContext.Orders.FirstOrDefaultAsync(x => x.PublicToken == token);
        if (order is null) return NotFound();
        if (order.PaymentStatus == PaymentStatus.Paid) return RedirectToAction("Confirmation", "Checkout", new { token });

        var transaction = new PaymentTransaction(order.Id, "Zarinpal", order.Total);
        DbContext.PaymentTransactions.Add(transaction);
        await DbContext.SaveChangesAsync();
        var callbackUrl = Url.Action(nameof(Callback), "Payment", new { token }, Request.Scheme)!;
        var payment = await _gateway.RequestAsync(order.Total, $"پرداخت سفارش {order.Number}", callbackUrl, order.Phone, order.Number, HttpContext.RequestAborted);
        if (!payment.Succeeded || string.IsNullOrWhiteSpace(payment.Authority) || string.IsNullOrWhiteSpace(payment.RedirectUrl))
        {
            transaction.MarkFailed(payment.Code, payment.Error);
            order.MarkPaymentFailed();
            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Result), new { token });
        }
        transaction.MarkRequested(payment.Authority, payment.Code);
        order.MarkPaymentPending();
        await DbContext.SaveChangesAsync();
        return Redirect(payment.RedirectUrl);
    }
}
