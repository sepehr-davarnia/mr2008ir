using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.Services;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ProjectsController : Controller
{
    private readonly AtelierDbContext _dbContext;

    public ProjectsController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .OrderByDescending(project => project.CreatedAt)
            .Select(project => new ProjectListItemViewModel
            {
                Id = project.Id,
                Title = project.Title,
                Slug = project.Slug,
                IsPublished = project.IsPublished
            })
            .ToListAsync();

        return View(projects);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProjectEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectEditViewModel model)
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

        var project = new Project(
            model.Title,
            slug,
            model.Description,
            model.FeaturedMediaId,
            model.IsPublished);

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "پروژه با موفقیت ذخیره شد.";
        return RedirectToAction("Edit", new { id = project.Id, area = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (project is null)
        {
            return NotFound();
        }

        var model = new ProjectEditViewModel
        {
            Id = project.Id,
            Title = project.Title,
            Slug = project.Slug,
            Description = project.Description,
            FeaturedMediaId = project.FeaturedMediaId,
            FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(project.FeaturedMediaId),
            IsPublished = project.IsPublished,
            Gallery = await LoadProjectGalleryAsync(project.Id)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        NormalizeSlug(model);

        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == model.Id.Value);
        if (project is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            model.Gallery = await LoadProjectGalleryAsync(model.Id.Value);
            return View(model);
        }

        if (!await ValidateFeaturedMediaAsync(model.FeaturedMediaId))
        {
            ModelState.AddModelError(nameof(model.FeaturedMediaId), MediaAltTextHelper.RequiredMessage);
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            model.Gallery = await LoadProjectGalleryAsync(model.Id.Value);
            return View(model);
        }

        var slug = await EnsureUniqueSlugAsync(model.Slug, model.Title, project.Id);
        if (slug is null)
        {
            ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            model.Gallery = await LoadProjectGalleryAsync(model.Id.Value);
            return View(model);
        }

        project.UpdateDetails(model.Title, model.Description, model.FeaturedMediaId, model.IsPublished);
        _dbContext.Entry(project).Property("Slug").CurrentValue = slug;
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "پروژه با موفقیت ویرایش شد.";
        return RedirectToAction("Edit", new { id = project.Id, area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGalleryMedia(int id, int newGalleryMediaId)
    {
        var project = await _dbContext.Projects
            .Include(item => item.Gallery)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (project is null)
        {
            return NotFound();
        }

        var media = await _dbContext.Media.FirstOrDefaultAsync(item => item.Id == newGalleryMediaId);
        if (media is null)
        {
            TempData["ErrorMessage"] = "فایل انتخاب شده یافت نشد.";
            return RedirectToAction("Edit", new { id, area = "Admin" });
        }

        if (!MediaAltTextHelper.IsImageContentType(media.ContentType))
        {
            TempData["ErrorMessage"] = "فقط می‌توانید تصاویر را به گالری اضافه کنید.";
            return RedirectToAction("Edit", new { id, area = "Admin" });
        }

        if (!MediaAltTextHelper.HasAltText(media.AltText))
        {
            TempData["ErrorMessage"] = "لطفاً برای تصویر انتخابی متن جایگزین ثبت کنید.";
            return RedirectToAction("Edit", new { id, area = "Admin" });
        }

        var existingProjectId = EF.Property<int?>(media, "ProjectId");
        if (existingProjectId.HasValue && existingProjectId.Value != id)
        {
            TempData["ErrorMessage"] = "این تصویر در گالری پروژه دیگری استفاده شده است.";
            return RedirectToAction("Edit", new { id, area = "Admin" });
        }

        if (project.Gallery.Any(item => item.Id == media.Id))
        {
            TempData["ErrorMessage"] = "این تصویر قبلاً به گالری اضافه شده است.";
            return RedirectToAction("Edit", new { id, area = "Admin" });
        }

        project.AddMedia(media);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "تصویر به گالری پروژه اضافه شد.";
        return RedirectToAction("Edit", new { id, area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveGalleryMedia(int id, int mediaId)
    {
        var project = await _dbContext.Projects
            .Include(item => item.Gallery)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (project is null)
        {
            return NotFound();
        }

        var media = project.Gallery.FirstOrDefault(item => item.Id == mediaId);
        if (media is null)
        {
            TempData["ErrorMessage"] = "تصویر در گالری یافت نشد.";
            return RedirectToAction("Edit", new { id, area = "Admin" });
        }

        project.RemoveMedia(media);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "تصویر از گالری حذف شد.";
        return RedirectToAction("Edit", new { id, area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == id);
        if (project is null)
        {
            return NotFound();
        }

        project.Publish();
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "پروژه منتشر شد.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == id);
        if (project is null)
        {
            return NotFound();
        }

        project.Unpublish();
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "پروژه به حالت پیش‌نویس بازگشت.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _dbContext.Projects.FirstOrDefaultAsync(item => item.Id == id);
        if (project is null)
        {
            return NotFound();
        }

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "پروژه با موفقیت حذف شد.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    private void NormalizeSlug(ProjectEditViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Slug))
        {
            return;
        }

        model.Slug = model.Title;
        ModelState.Remove(nameof(model.Slug));
    }

    private async Task<string?> EnsureUniqueSlugAsync(string? requestedSlug, string title, int? projectId)
    {
        var slugSource = string.IsNullOrWhiteSpace(requestedSlug) ? title : requestedSlug;
        var slug = GenerateSlug(slugSource);

        var isDuplicate = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(project => project.Slug == slug && project.Id != projectId);

        return isDuplicate ? null : slug;
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return $"project-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(slug)
            ? $"project-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : slug;
    }

    private async Task<string?> GetFeaturedMediaUrlAsync(int? mediaId)
    {
        if (!mediaId.HasValue)
        {
            return null;
        }

        var media = await _dbContext.Media.AsNoTracking().FirstOrDefaultAsync(item => item.Id == mediaId.Value);
        if (media is null)
        {
            return null;
        }

        if (media.StorageId is null)
        {
            return media.Url;
        }

        return Url.Action("Serve", "Media", new { area = "Admin", mediaId }) ?? media.Url;
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

    private async Task<IReadOnlyList<ProjectGalleryItemViewModel>> LoadProjectGalleryAsync(int projectId)
    {
        return await _dbContext.Media
            .AsNoTracking()
            .Where(media => EF.Property<int?>(media, "ProjectId") == projectId)
            .OrderBy(media => media.CreatedAt)
            .Select(media => new ProjectGalleryItemViewModel
            {
                Id = media.Id,
                Url = media.StorageId.HasValue
                    ? Url.Action("Serve", "Media", new { area = "Admin", mediaId = media.Id }) ?? media.Url
                    : media.Url,
                AltText = media.AltText ?? string.Empty
            })
            .ToListAsync();
    }
}
