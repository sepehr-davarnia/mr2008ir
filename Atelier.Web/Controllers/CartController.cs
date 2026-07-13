using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

[Route("cart")]
public class CartController : PublicControllerBase
{
    private readonly ICartService _cart;
    private readonly ICartViewModelBuilder _cartBuilder;

    public CartController(AtelierDbContext dbContext, ICartService cart, ICartViewModelBuilder cartBuilder) : base(dbContext)
    {
        _cart = cart;
        _cartBuilder = cartBuilder;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _cartBuilder.BuildAsync();
        ViewData["Robots"] = "noindex,nofollow";
        SetSeoMetadata("سبد خرید | mr2008.ir", "مدیریت قطعات انتخاب‌شده و ادامه ثبت سفارش.");
        return View(model);
    }

    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        var purchasable = await DbContext.Products.AsNoTracking().AnyAsync(product =>
            product.Id == productId && product.Status == ProductStatus.Published &&
            product.PriceType == PriceType.Fixed && product.Price > 0);
        if (!purchasable) return BadRequest("این محصول در حال حاضر امکان خرید آنلاین ندارد.");

        _cart.Add(productId, quantity);
        if (Request.Headers.Accept.Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true))
            return Json(new { ok = true, count = _cart.Count, message = "محصول به سبد خرید اضافه شد." });

        TempData["CartMessage"] = "محصول به سبد خرید اضافه شد.";
        return LocalRedirect(IsLocalUrl(returnUrl) ? returnUrl! : "/cart");
    }

    [HttpPost("update")]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int quantity)
    {
        _cart.Update(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("remove")]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _cart.Remove(productId);
        TempData["CartMessage"] = "محصول از سبد خرید حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private bool IsLocalUrl(string? url) => !string.IsNullOrWhiteSpace(url) && Url.IsLocalUrl(url);
}
