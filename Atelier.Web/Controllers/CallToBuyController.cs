using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

[Route("call-to-buy")]
public class CallToBuyController : Controller
{
    private readonly AtelierDbContext _dbContext;
    public CallToBuyController(AtelierDbContext dbContext) => _dbContext = dbContext;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("contact")]
    public async Task<IActionResult> Submit(CallToBuyInputModel input)
    {
        var product = await _dbContext.Products.AsNoTracking()
            .Where(item => item.Id == input.ProductId && item.Status == ProductStatus.Published)
            .Select(item => new { item.Name, item.Slug }).FirstOrDefaultAsync();
        if (product is null) return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["CallError"] = "نام و شماره موبایل معتبر را وارد کنید.";
            return LocalRedirect(SafeReturnUrl(input.ReturnUrl));
        }

        var message = $"درخواست تماس برای خرید: {product.Name}\nشماره تماس: {input.Phone.Trim()}";
        if (!string.IsNullOrWhiteSpace(input.Note)) message += $"\nتوضیح: {input.Note.Trim()}";
        _dbContext.Leads.Add(new Lead(input.Name.Trim(), "call-to-buy@mr2008.ir", message));
        await _dbContext.SaveChangesAsync();
        TempData["CallSuccess"] = "درخواست شما ثبت شد؛ کارشناس فروش در اولین فرصت تماس می‌گیرد.";
        return LocalRedirect(SafeReturnUrl(input.ReturnUrl));
    }

    private string SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/categories";
}
