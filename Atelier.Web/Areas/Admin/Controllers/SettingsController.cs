using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.Services;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

public class SettingsController : AdminController
{
    private readonly AtelierDbContext _dbContext;

    public SettingsController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var settings = await UploadSettingsHelper.GetOrCreateSettingsAsync(_dbContext);

        var model = new SettingsViewModel
        {
            SiteName = settings.SiteName,
            Address = settings.Address,
            Phone = settings.Phone,
            Mobile = settings.Mobile,
            WhatsApp = settings.WhatsApp,
            Instagram = settings.Instagram,
            Telegram = settings.Telegram,
            Email = settings.Email,
            LogoMediaId = settings.LogoMediaId,
            FaviconMediaId = settings.FaviconMediaId,
            HomeHeroMediaId = settings.HomeHeroMediaId,
            HomeSecondaryMediaId = settings.HomeSecondaryMediaId,
            DefaultCategoryMediaId = settings.DefaultCategoryMediaId,
            MaxUploadSizeKb = settings.MaxUploadSizeKb,
            LogoPreviewUrl = await GetMediaPreviewUrlAsync(settings.LogoMediaId),
            FaviconPreviewUrl = await GetMediaPreviewUrlAsync(settings.FaviconMediaId),
            HomeHeroPreviewUrl = await GetMediaPreviewUrlAsync(settings.HomeHeroMediaId),
            HomeSecondaryPreviewUrl = await GetMediaPreviewUrlAsync(settings.HomeSecondaryMediaId),
            DefaultCategoryPreviewUrl = await GetMediaPreviewUrlAsync(settings.DefaultCategoryMediaId)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.LogoPreviewUrl = await GetMediaPreviewUrlAsync(model.LogoMediaId);
            model.FaviconPreviewUrl = await GetMediaPreviewUrlAsync(model.FaviconMediaId);
            model.HomeHeroPreviewUrl = await GetMediaPreviewUrlAsync(model.HomeHeroMediaId);
            model.HomeSecondaryPreviewUrl = await GetMediaPreviewUrlAsync(model.HomeSecondaryMediaId);
            model.DefaultCategoryPreviewUrl = await GetMediaPreviewUrlAsync(model.DefaultCategoryMediaId);
            return View(model);
        }

        var logoValidation = await ValidateMediaAsync(model.LogoMediaId, allowPngOnly: false);
        if (!logoValidation.IsValid)
        {
            ModelState.AddModelError(nameof(model.LogoMediaId), logoValidation.ErrorMessage ?? "لطفاً یک تصویر معتبر برای لوگو انتخاب کنید.");
        }

        var faviconValidation = await ValidateMediaAsync(model.FaviconMediaId, allowPngOnly: true);
        if (!faviconValidation.IsValid)
        {
            ModelState.AddModelError(nameof(model.FaviconMediaId), faviconValidation.ErrorMessage ?? "فاوآیکن باید تصویر PNG باشد.");
        }

        var heroValidation = await ValidateMediaAsync(model.HomeHeroMediaId, allowPngOnly: false);
        if (!heroValidation.IsValid)
        {
            ModelState.AddModelError(nameof(model.HomeHeroMediaId), heroValidation.ErrorMessage ?? "لطفاً یک تصویر معتبر برای هیرو انتخاب کنید.");
        }

        var secondaryValidation = await ValidateMediaAsync(model.HomeSecondaryMediaId, allowPngOnly: false);
        if (!secondaryValidation.IsValid)
        {
            ModelState.AddModelError(nameof(model.HomeSecondaryMediaId), secondaryValidation.ErrorMessage ?? "لطفاً یک تصویر معتبر برای تصویر ثانویه انتخاب کنید.");
        }

        var defaultCategoryValidation = await ValidateMediaAsync(model.DefaultCategoryMediaId, allowPngOnly: false);
        if (!defaultCategoryValidation.IsValid)
        {
            ModelState.AddModelError(nameof(model.DefaultCategoryMediaId), defaultCategoryValidation.ErrorMessage ?? "لطفاً یک تصویر معتبر برای پیش فرض دسته بندی انتخاب کنید.");
        }

        if (!ModelState.IsValid)
        {
            model.LogoPreviewUrl = await GetMediaPreviewUrlAsync(model.LogoMediaId);
            model.FaviconPreviewUrl = await GetMediaPreviewUrlAsync(model.FaviconMediaId);
            model.HomeHeroPreviewUrl = await GetMediaPreviewUrlAsync(model.HomeHeroMediaId);
            model.HomeSecondaryPreviewUrl = await GetMediaPreviewUrlAsync(model.HomeSecondaryMediaId);
            model.DefaultCategoryPreviewUrl = await GetMediaPreviewUrlAsync(model.DefaultCategoryMediaId);
            return View(model);
        }

        var settings = await UploadSettingsHelper.GetOrCreateSettingsAsync(_dbContext);
        var maxUploadSizeKb = UploadSettingsHelper.NormalizeMaxUploadSizeKb(model.MaxUploadSizeKb ?? 0);
        settings.UpdateMaxUploadSizeKb(maxUploadSizeKb);
        settings.UpdateContactInfo(
            model.SiteName,
            model.Address,
            model.Phone,
            model.Mobile,
            model.WhatsApp,
            model.Instagram,
            model.Telegram,
            model.Email);
        settings.UpdateBranding(model.LogoMediaId, model.FaviconMediaId);
        settings.UpdateVisualMedia(model.HomeHeroMediaId, model.HomeSecondaryMediaId, model.DefaultCategoryMediaId);

        await _dbContext.SaveChangesAsync();

        model.MaxUploadSizeKb = maxUploadSizeKb;
        model.LogoPreviewUrl = await GetMediaPreviewUrlAsync(model.LogoMediaId);
        model.FaviconPreviewUrl = await GetMediaPreviewUrlAsync(model.FaviconMediaId);
        model.HomeHeroPreviewUrl = await GetMediaPreviewUrlAsync(model.HomeHeroMediaId);
        model.HomeSecondaryPreviewUrl = await GetMediaPreviewUrlAsync(model.HomeSecondaryMediaId);
        model.DefaultCategoryPreviewUrl = await GetMediaPreviewUrlAsync(model.DefaultCategoryMediaId);

        TempData["SuccessMessage"] = "تنظیمات با موفقیت ذخیره شد.";
        return View(model);
    }

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateMediaAsync(int? mediaId, bool allowPngOnly)
    {
        if (!mediaId.HasValue)
        {
            return (true, null);
        }

        var media = await _dbContext.Media.AsNoTracking().FirstOrDefaultAsync(item => item.Id == mediaId.Value);
        if (media?.ContentType is null)
        {
            return (false, "لطفاً یک تصویر معتبر انتخاب کنید.");
        }

        if (allowPngOnly)
        {
            var isPng = string.Equals(media.ContentType, "image/png", StringComparison.OrdinalIgnoreCase);
            return isPng
                ? (true, null)
                : (false, "فاوآیکن باید تصویر PNG باشد.");
        }

        if (!MediaAltTextHelper.IsImageContentType(media.ContentType))
        {
            return (false, "لطفاً یک تصویر معتبر انتخاب کنید.");
        }

        if (!MediaAltTextHelper.HasAltText(media.AltText))
        {
            return (false, MediaAltTextHelper.RequiredMessage);
        }

        return (true, null);
    }

    private async Task<string?> GetMediaPreviewUrlAsync(int? mediaId)
    {
        if (!mediaId.HasValue)
        {
            return null;
        }

        return await _dbContext.Media
            .AsNoTracking()
            .Where(item => item.Id == mediaId.Value)
            .Select(item => item.Url)
            .FirstOrDefaultAsync();
    }
}
