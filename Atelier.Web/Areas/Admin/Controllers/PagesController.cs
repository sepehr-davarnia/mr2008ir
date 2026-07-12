using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.Services;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PagesController : Controller
{
    private readonly AtelierDbContext _dbContext;

    public PagesController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var pages = await _dbContext.Pages
            .AsNoTracking()
            .OrderByDescending(page => page.CreatedAt)
            .Select(page => new PageListItemViewModel
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                Status = page.Status
            })
            .ToListAsync();

        return View(pages);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new PageEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageEditViewModel model)
    {
        NormalizeSlug(model);

        if (!ModelState.IsValid)
        {
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            return View(model);
        }

        if (!await ValidateFeaturedMediaAsync(model.FeaturedMediaId))
        {
            ModelState.AddModelError(nameof(model.FeaturedMediaId), MediaAltTextHelper.RequiredMessage);
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            return View(model);
        }

        var slug = await EnsureUniqueSlugAsync(model.Slug, model.Title, null);
        if (slug is null)
        {
            ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            return View(model);
        }

        var page = new Page(
            model.Title,
            slug,
            model.Content,
            model.FeaturedMediaId,
            model.MetaTitle,
            model.MetaDescription);

        _dbContext.Pages.Add(page);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "صفحه با موفقیت ذخیره شد.";
        return RedirectToAction("Edit", new { id = page.Id, area = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var page = await _dbContext.Pages.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (page is null)
        {
            return NotFound();
        }

        var model = new PageEditViewModel
        {
            Id = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            Content = page.Content,
            FeaturedMediaId = page.FeaturedMediaId,
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(page.FeaturedMediaId)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PageEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        NormalizeSlug(model);

        var page = await _dbContext.Pages.FirstOrDefaultAsync(item => item.Id == model.Id.Value);
        if (page is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            return View(model);
        }

        if (!await ValidateFeaturedMediaAsync(model.FeaturedMediaId))
        {
            ModelState.AddModelError(nameof(model.FeaturedMediaId), MediaAltTextHelper.RequiredMessage);
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            return View(model);
        }

        var slug = await EnsureUniqueSlugAsync(model.Slug, model.Title, page.Id);
        if (slug is null)
        {
            ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            return View(model);
        }

        page.UpdateContent(model.Title, model.Content, model.FeaturedMediaId, model.MetaTitle, model.MetaDescription);
        _dbContext.Entry(page).Property("Slug").CurrentValue = slug;
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "صفحه با موفقیت ویرایش شد.";
        return RedirectToAction("Edit", new { id = page.Id, area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var page = await _dbContext.Pages.FirstOrDefaultAsync(item => item.Id == id);
        if (page is null)
        {
            return NotFound();
        }

        page.Publish();
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "صفحه منتشر شد.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id)
    {
        var page = await _dbContext.Pages.FirstOrDefaultAsync(item => item.Id == id);
        if (page is null)
        {
            return NotFound();
        }

        page.Unpublish();
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "صفحه به حالت پیش‌نویس بازگشت.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var page = await _dbContext.Pages.FirstOrDefaultAsync(item => item.Id == id);
        if (page is null)
        {
            return NotFound();
        }

        _dbContext.Pages.Remove(page);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "صفحه با موفقیت حذف شد.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    private void NormalizeSlug(PageEditViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Slug))
        {
            return;
        }

        model.Slug = model.Title;
        ModelState.Remove(nameof(model.Slug));
    }

    private async Task<string?> EnsureUniqueSlugAsync(string? requestedSlug, string title, int? pageId)
    {
        var slugSource = string.IsNullOrWhiteSpace(requestedSlug) ? title : requestedSlug;
        var slug = GenerateSlug(slugSource);

        var isDuplicate = await _dbContext.Pages
            .AsNoTracking()
            .AnyAsync(page => page.Slug == slug && page.Id != pageId);

        return isDuplicate ? null : slug;
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return $"page-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(slug)
            ? $"page-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : slug;
    }

    private async Task<string?> GetFeaturedMediaUrlAsync(int? mediaId)
    {
        if (!mediaId.HasValue)
        {
            return null;
        }

        var exists = await _dbContext.Media.AnyAsync(media => media.Id == mediaId.Value);
        return exists ? Url.Action("Serve", "Media", new { area = "Admin", mediaId }) : null;
    }

    private async Task<bool> ValidateFeaturedMediaAsync(int? mediaId)
    {
        if (!mediaId.HasValue)
        {
            return true;
        }

        var media = await _dbContext.Media.AsNoTracking().FirstOrDefaultAsync(item => item.Id == mediaId.Value);
        if (media?.ContentType is null)
        {
            return false;
        }

        if (!MediaAltTextHelper.IsImageContentType(media.ContentType))
        {
            return false;
        }

        return MediaAltTextHelper.HasAltText(media.AltText);
    }
}
