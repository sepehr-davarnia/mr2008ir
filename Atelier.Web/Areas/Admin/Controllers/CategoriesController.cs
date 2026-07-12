using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.Services;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CategoriesController : Controller
{
    private readonly AtelierDbContext _dbContext;

    public CategoriesController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .OrderByDescending(category => category.CreatedAt)
            .Select(category => new CategoryListItemViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                IsActive = true
            })
            .ToListAsync();

        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoryEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryEditViewModel model)
    {
        NormalizeSlug(model);

        if (!ModelState.IsValid)
        {
            model.MediaPreviewUrl = await GetMediaPreviewUrlAsync(model.MediaId);
            return View(model);
        }

        var mediaValidation = await ValidateMediaAsync(model.MediaId);
        if (!mediaValidation.IsValid)
        {
            ModelState.AddModelError(nameof(model.MediaId), mediaValidation.ErrorMessage ?? MediaAltTextHelper.RequiredMessage);
            model.MediaPreviewUrl = await GetMediaPreviewUrlAsync(model.MediaId);
            return View(model);
        }

        var slug = await EnsureUniqueSlugAsync(model.Slug, model.Name, null);
        if (slug is null)
        {
            ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
            model.MediaPreviewUrl = await GetMediaPreviewUrlAsync(model.MediaId);
            return View(model);
        }

        var category = new Category(model.Name, slug, null, model.MediaId);
        _dbContext.Categories.Add(category);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "خطا در انجام عملیات. لطفاً دوباره تلاش کنید.";
            return View(model);
        }

        TempData["SuccessMessage"] = "دسته‌بندی با موفقیت ذخیره شد.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (category is null)
        {
            return NotFound();
        }

        var model = new CategoryEditViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            MediaId = category.MediaId,
            MediaPreviewUrl = await GetMediaPreviewUrlAsync(category.MediaId)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        NormalizeSlug(model);

        var category = await _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == model.Id.Value);
        if (category is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.MediaPreviewUrl = await GetMediaPreviewUrlAsync(model.MediaId);
            return View(model);
        }

        var mediaValidation = await ValidateMediaAsync(model.MediaId);
        if (!mediaValidation.IsValid)
        {
            ModelState.AddModelError(nameof(model.MediaId), mediaValidation.ErrorMessage ?? MediaAltTextHelper.RequiredMessage);
            model.MediaPreviewUrl = await GetMediaPreviewUrlAsync(model.MediaId);
            return View(model);
        }

        var slug = await EnsureUniqueSlugAsync(model.Slug, model.Name, category.Id);
        if (slug is null)
        {
            ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
            model.MediaPreviewUrl = await GetMediaPreviewUrlAsync(model.MediaId);
            return View(model);
        }

        category.UpdateName(model.Name);
        category.UpdateMedia(model.MediaId);
        _dbContext.Entry(category).Property("Slug").CurrentValue = slug;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "خطا در انجام عملیات. لطفاً دوباره تلاش کنید.";
            return View(model);
        }

        TempData["SuccessMessage"] = "دسته‌بندی با موفقیت ویرایش شد.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(item => item.Id == id);
        if (category is null)
        {
            return NotFound();
        }

        _dbContext.Categories.Remove(category);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "خطا در انجام عملیات. لطفاً دوباره تلاش کنید.";
            return RedirectToAction("Index", new { area = "Admin" });
        }

        TempData["SuccessMessage"] = "دسته‌بندی با موفقیت حذف شد.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    private void NormalizeSlug(CategoryEditViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Slug))
        {
            return;
        }

        model.Slug = model.Name;
        ModelState.Remove(nameof(model.Slug));
    }

    private async Task<string?> EnsureUniqueSlugAsync(string? requestedSlug, string name, int? categoryId)
    {
        var slugSource = string.IsNullOrWhiteSpace(requestedSlug) ? name : requestedSlug;
        var slug = GenerateSlug(slugSource);

        var isDuplicate = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(category => category.Slug == slug && category.Id != categoryId);

        return isDuplicate ? null : slug;
    }

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateMediaAsync(int? mediaId)
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

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return $"category-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(slug)
            ? $"category-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : slug;
    }
}
