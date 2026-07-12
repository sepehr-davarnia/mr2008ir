using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.Services;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class BlogController : Controller
{
    private readonly AtelierDbContext _dbContext;

    public BlogController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var posts = await _dbContext.BlogPosts
            .AsNoTracking()
            .OrderByDescending(post => post.CreatedAt)
            .Select(post => new BlogPostListItemViewModel
            {
                Id = post.Id,
                Title = post.Title,
                PublishedAt = post.PublishedAt
            })
            .ToListAsync();

        var model = new BlogPostIndexViewModel
        {
            Posts = posts
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(new BlogPostFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPostFormViewModel model)
    {
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

        var post = new BlogPost(
            model.Title,
            slug,
            model.Excerpt,
            model.Content,
            model.FeaturedMediaId,
            model.MetaTitle,
            model.MetaDescription);

        _dbContext.BlogPosts.Add(post);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "مقاله با موفقیت ذخیره شد.";

        return RedirectToAction("Edit", new { id = post.Id, area = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _dbContext.BlogPosts.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);

        if (post is null)
        {
            return NotFound();
        }

        var model = new BlogPostFormViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Excerpt = post.Excerpt,
            Content = post.Content,
            FeaturedMediaId = post.FeaturedMediaId,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(post.FeaturedMediaId)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BlogPostFormViewModel model)
    {
        if (model.Id is null)
        {
            return NotFound();
        }

        var post = await _dbContext.BlogPosts.FirstOrDefaultAsync(item => item.Id == model.Id);
        if (post is null)
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

        var slug = await EnsureUniqueSlugAsync(model.Slug, model.Title, post.Id);
        if (slug is null)
        {
            ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
            model.FeaturedMediaUrl = await GetFeaturedMediaUrlAsync(model.FeaturedMediaId);
            return View(model);
        }

        post.UpdateDetails(
            model.Title,
            slug,
            model.Excerpt,
            model.Content,
            model.FeaturedMediaId,
            model.MetaTitle,
            model.MetaDescription);

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "مقاله با موفقیت ذخیره شد.";

        return RedirectToAction("Edit", new { id = post.Id, area = "Admin" });
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var post = await _dbContext.BlogPosts.FirstOrDefaultAsync(item => item.Id == id);
        if (post is null)
        {
            return NotFound();
        }

        post.Publish();
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "مقاله منتشر شد.";

        return RedirectToAction("Index", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id)
    {
        var post = await _dbContext.BlogPosts.FirstOrDefaultAsync(item => item.Id == id);
        if (post is null)
        {
            return NotFound();
        }

        post.Unpublish();
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "مقاله غیر فعال شد.";

        return RedirectToAction("Index", new { area = "Admin" });
    }

    private async Task<string?> EnsureUniqueSlugAsync(string? requestedSlug, string title, int? postId)
    {
        var slugSource = string.IsNullOrWhiteSpace(requestedSlug) ? title : requestedSlug;

        var isDuplicate = await _dbContext.BlogPosts.AnyAsync(post => post.Slug == slugSource && post.Id != postId);

        return $"blog-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }


    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return $"blog-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(slug) ? $"blog-{DateTime.UtcNow:yyyyMMddHHmmss}" : slug;
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
