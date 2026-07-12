using Atelier.Infrastructure.Data;
using Atelier.Infrastructure.Data.Entities;
using Atelier.Web.Areas.Admin.Services;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class MediaManagerController : Controller
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly AtelierDbContext _dbContext;

    public MediaManagerController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private const int PageSize = 60;

    [HttpGet]
    public async Task<IActionResult> Picker(int? selectedMediaId, int page = 1)
    {
        page = Math.Max(1, page);
        var settings = await UploadSettingsHelper.GetOrCreateSettingsAsync(_dbContext);

        var query = _dbContext.Media
            .AsNoTracking()
            .Where(media => media.ContentType != null && AllowedContentTypes.Contains(media.ContentType))
            .OrderByDescending(media => media.CreatedAt);

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(media => new
            {
                media.Id,
                media.Url,
                Title = string.IsNullOrWhiteSpace(media.Title) ? media.Slug : media.Title!,
                media.AltText
            })
            .ToListAsync();

        var model = new MediaManagerPickerViewModel
        {
            Items = items.Select(item => new MediaManagerItemViewModel
            {
                Id = item.Id,
                ThumbnailUrl = item.Url,
                Title = item.Title,
                AltText = item.AltText,
                SizedUrl = BuildSizedUrl(item.Url, item.Id, 480),
                SrcSet = BuildSrcSet(item.Url, item.Id)
            }).ToList(),
            MaxUploadSizeDisplay = UploadSettingsHelper.FormatMegabytes(settings.MaxUploadSizeKb),
            Page = page,
            PageSize = PageSize,
            HasMore = totalItems > page * PageSize
        };

        ViewData["SelectedMediaId"] = selectedMediaId;
        return PartialView("_Picker", model);
    }

    [HttpGet]
    public async Task<IActionResult> List(int page = 1)
    {
        page = Math.Max(1, page);
        var query = _dbContext.Media
            .AsNoTracking()
            .Where(media => media.ContentType != null && AllowedContentTypes.Contains(media.ContentType))
            .OrderByDescending(media => media.CreatedAt);

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(media => new
            {
                media.Id,
                media.Url,
                Title = string.IsNullOrWhiteSpace(media.Title) ? media.Slug : media.Title!,
                media.AltText
            })
            .ToListAsync();

        var response = new
        {
            items = items.Select(item => new
            {
                id = item.Id,
                url = item.Url,
                sizedUrl = BuildSizedUrl(item.Url, item.Id, 480),
                srcSet = BuildSrcSet(item.Url, item.Id),
                title = item.Title,
                altText = item.AltText,
                hasAltText = !string.IsNullOrWhiteSpace(item.AltText)
            }),
            hasMore = totalItems > page * PageSize,
            nextPage = page + 1
        };

        return Json(response);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file, string? title, string? altText)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "لطفاً یک فایل معتبر انتخاب کنید." });
        }

        var settings = await UploadSettingsHelper.GetOrCreateSettingsAsync(_dbContext);
        var maxUploadBytes = UploadSettingsHelper.ToBytes(settings.MaxUploadSizeKb);
        if (file.Length > maxUploadBytes)
        {
            var display = UploadSettingsHelper.FormatMegabytes(settings.MaxUploadSizeKb);
            return BadRequest(new { message = $"حجم فایل نباید بیشتر از {display} مگابایت باشد." });
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(new { message = "فرمت فایل باید تصویر باشد." });
        }

        if (string.IsNullOrWhiteSpace(altText))
        {
            return BadRequest(new { message = "وارد کردن متن جایگزین تصویر (Alt Text) برای سئو الزامی است." });
        }

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var mediaContent = new MediaContent
        {
            Data = memoryStream.ToArray()
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.MediaContents.Add(mediaContent);
        await _dbContext.SaveChangesAsync();

        var slug = await GenerateUniqueSlugAsync(Path.GetFileNameWithoutExtension(file.FileName));

        var media = new Media(
            slug,
            string.Empty,
            mediaContent.Id,
            file.ContentType,
            file.Length,
            title,
            altText,
            file.FileName);

        _dbContext.Media.Add(media);
        await _dbContext.SaveChangesAsync();

        var url = Url.Action("Serve", "Media", new { area = "Admin", mediaId = media.Id }) ?? string.Empty;
        media.UpdateUrl(url);
        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return Json(new
        {
            mediaId = media.Id,
            thumbnailUrl = url,
            sizedUrl = BuildSizedUrl(url, media.Id, 480),
            srcSet = BuildSrcSet(url, media.Id),
            title = string.IsNullOrWhiteSpace(media.Title) ? media.Slug : media.Title,
            altText = media.AltText
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAltText(int mediaId, string? altText)
    {
        if (string.IsNullOrWhiteSpace(altText))
        {
            return BadRequest(new { message = "وارد کردن متن جایگزین تصویر (Alt Text) برای سئو الزامی است." });
        }

        var media = await _dbContext.Media.FirstOrDefaultAsync(item => item.Id == mediaId);
        if (media is null)
        {
            return NotFound(new { message = "تصویر انتخاب شده پیدا نشد." });
        }

        media.UpdateMetadata(media.Title, altText);
        await _dbContext.SaveChangesAsync();

        return Json(new
        {
            altText = media.AltText
        });
    }

    private string BuildSizedUrl(string url, int mediaId, int width)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        if (url.Contains("/Admin/Media/Serve", StringComparison.OrdinalIgnoreCase))
        {
            var sizedUrl = Url.Action("Serve", "Media", new { area = "Admin", mediaId, width, format = "webp" });
            return sizedUrl ?? url;
        }

        return url;
    }

    private string BuildSrcSet(string url, int mediaId)
    {
        var widths = new[] { 320, 640, 960 };
        var sized = widths
            .Select(width => $"{BuildSizedUrl(url, mediaId, width)} {width}w");

        return string.Join(", ", sized);
    }

    private async Task<string> GenerateUniqueSlugAsync(string input)
    {
        var baseSlug = GenerateSlug(input);
        var uniqueSlug = baseSlug;
        var suffix = 1;

        while (await _dbContext.Media.AsNoTracking().AnyAsync(m => m.Slug == uniqueSlug))
        {
            uniqueSlug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return uniqueSlug;
    }

    private static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return $"media-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(slug)
            ? $"media-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : slug;
    }
}
