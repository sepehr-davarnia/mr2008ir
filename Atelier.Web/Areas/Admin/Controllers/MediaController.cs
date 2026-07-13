using Atelier.Infrastructure.Data;
using Atelier.Infrastructure.Data.Entities;
using Atelier.Web.Areas.Admin.Services;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class MediaController : Controller
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf"
    ];

    private readonly AtelierDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public MediaController(AtelierDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _dbContext.Media
            .AsNoTracking()
            .OrderByDescending(media => media.CreatedAt)
            .Select(media => new MediaListItemViewModel
            {
                Id = media.Id,
                Title = media.Title,
                AltText = media.AltText,
                ContentType = media.ContentType,
                FileSize = media.FileSize,
                ProductId = EF.Property<int?>(media, "ProductId")
            })
            .ToListAsync();

        var model = new MediaListViewModel
        {
            Items = items
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Upload(int productId)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
        {
            return NotFound();
        }

        var settings = await UploadSettingsHelper.GetOrCreateSettingsAsync(_dbContext);

        ViewData["ProductName"] = product.Name;

        var model = new MediaUploadViewModel
        {
            ProductId = productId,
            MaxUploadSizeDisplay = UploadSettingsHelper.FormatMegabytes(settings.MaxUploadSizeKb)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(MediaUploadViewModel model)
    {
        var product = await _dbContext.Products
            .Include(p => p.Gallery)
            .FirstOrDefaultAsync(p => p.Id == model.ProductId);

        if (product is null)
        {
            return NotFound();
        }

        var settings = await UploadSettingsHelper.GetOrCreateSettingsAsync(_dbContext);
        var maxUploadBytes = UploadSettingsHelper.ToBytes(settings.MaxUploadSizeKb);
        var maxUploadDisplay = UploadSettingsHelper.FormatMegabytes(settings.MaxUploadSizeKb);

        if (model.File is null || model.File.Length == 0)
        {
            ModelState.AddModelError(nameof(model.File), "لطفاً یک فایل معتبر انتخاب کنید.");
            ViewData["ProductName"] = product.Name;
            model.MaxUploadSizeDisplay = maxUploadDisplay;
            return View(model);
        }

        if (model.File.Length > maxUploadBytes)
        {
            ModelState.AddModelError(nameof(model.File), $"حجم فایل نباید بیشتر از {maxUploadDisplay} مگابایت باشد.");
            ViewData["ProductName"] = product.Name;
            model.MaxUploadSizeDisplay = maxUploadDisplay;
            return View(model);
        }

        if (!AllowedContentTypes.Contains(model.File.ContentType))
        {
            ModelState.AddModelError(nameof(model.File), "فرمت فایل باید تصویر یا PDF باشد.");
            ViewData["ProductName"] = product.Name;
            model.MaxUploadSizeDisplay = maxUploadDisplay;
            return View(model);
        }

        if (MediaAltTextHelper.IsImageContentType(model.File.ContentType) && !MediaAltTextHelper.HasAltText(model.AltText))
        {
            ModelState.AddModelError(nameof(model.AltText), MediaAltTextHelper.RequiredMessage);
            ViewData["ProductName"] = product.Name;
            model.MaxUploadSizeDisplay = maxUploadDisplay;
            return View(model);
        }

        await using var memoryStream = new MemoryStream();
        await model.File.CopyToAsync(memoryStream);

        var mediaContent = new MediaContent
        {
            Data = memoryStream.ToArray()
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.MediaContents.Add(mediaContent);
        await _dbContext.SaveChangesAsync();

        var slug = await GenerateUniqueSlugAsync(Path.GetFileNameWithoutExtension(model.File.FileName));
        var contentType = model.File.ContentType;

        var media = new Media(
            slug,
            string.Empty,
            mediaContent.Id,
            contentType,
            model.File.Length,
            model.Title,
            model.AltText,
            model.File.FileName,
            "Product");

        product.AddMedia(media);
        _dbContext.Media.Add(media);
        await _dbContext.SaveChangesAsync();

        var url = Url.Action("Serve", "Media", new { area = "Admin", mediaId = media.Id }) ?? string.Empty;
        media.UpdateUrl(url);
        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        TempData["SuccessMessage"] = "تصویر با موفقیت ذخیره شد.";

        return RedirectToAction("Edit", "Products", new { area = "Admin", id = product.Id });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Serve(int mediaId, int? width = null, int? height = null, string? format = null)
    {
        var media = await _dbContext.Media
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media is null)
        {
            return NotFound();
        }

        if (media.StorageId is null)
        {
            return NotFound();
        }

        var content = await _dbContext.MediaContents.AsNoTracking().FirstOrDefaultAsync(c => c.Id == media.StorageId);

        if (content is null)
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "public,max-age=604800";

        var isImage = MediaAltTextHelper.IsImageContentType(media.ContentType);
        var hasSizing = width.HasValue || height.HasValue || !string.IsNullOrWhiteSpace(format);
        if (!isImage || !hasSizing)
        {
            return File(content.Data, media.ContentType ?? "application/octet-stream");
        }

        var targetWidth = width.HasValue ? Math.Clamp(width.Value, 64, 2400) : (int?)null;
        var targetHeight = height.HasValue ? Math.Clamp(height.Value, 64, 2400) : (int?)null;
        var detectedFormat = await Image.DetectFormatAsync(new MemoryStream(content.Data));
        var image = await Image.LoadAsync(new MemoryStream(content.Data));

        if (targetWidth.HasValue || targetHeight.HasValue)
        {
            image.Mutate(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(targetWidth ?? 0, targetHeight ?? 0),
                    Mode = ResizeMode.Max
                });
            });
        }

        (IImageEncoder encoder, string mimeType) = ResolveEncoder(format, detectedFormat);

        await using var output = new MemoryStream();
        image.Save(output, encoder);
        output.Position = 0;
        return File(output.ToArray(), mimeType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExternalImages(CancellationToken cancellationToken)
    {
        const int maxBytes = 10 * 1024 * 1024;
        var candidates = await _dbContext.Media
            .Where(media => media.StorageId == null && media.Purpose == "محصول" &&
                (media.Url.StartsWith("https://www.partonlineco.com/") || media.Url.StartsWith("https://partonlineco.com/")))
            .OrderBy(media => media.Id).Take(50).ToListAsync(cancellationToken);
        var client = _httpClientFactory.CreateClient("external-media");
        var imported = 0;
        var failed = 0;

        foreach (var media in candidates)
        {
            try
            {
                using var response = await client.GetAsync(media.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType is null || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
                    response.Content.Headers.ContentLength > maxBytes) throw new InvalidOperationException("Invalid remote image.");

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (bytes.Length == 0 || bytes.Length > maxBytes) throw new InvalidOperationException("Invalid image size.");

                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                var stored = new MediaContent { Data = bytes };
                _dbContext.MediaContents.Add(stored);
                await _dbContext.SaveChangesAsync(cancellationToken);
                media.AttachStoredContent(stored.Id, contentType, bytes.LongLength,
                    $"/Admin/Media/Serve?mediaId={media.Id}");
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                imported++;
            }
            catch
            {
                await _dbContext.Entry(media).ReloadAsync(cancellationToken);
                media.UpdateDownloadStatus(true);
                await _dbContext.SaveChangesAsync(cancellationToken);
                failed++;
            }
        }

        TempData["SuccessMessage"] = $"{imported} تصویر در پایگاه داده ذخیره شد. {failed} مورد ناموفق بود.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int productId, int mediaId)
    {
        var product = await _dbContext.Products
            .Include(p => p.Gallery)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
        {
            return NotFound();
        }

        var media = product.Gallery.FirstOrDefault(item => item.Id == mediaId);
        if (media is null)
        {
            return NotFound();
        }

        var content = media.StorageId is null
            ? null
            : await _dbContext.MediaContents.FirstOrDefaultAsync(c => c.Id == media.StorageId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        product.RemoveMedia(media);
        _dbContext.Media.Remove(media);

        if (content is not null)
        {
            _dbContext.MediaContents.Remove(content);
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["SuccessMessage"] = "تصویر با موفقیت حذف شد.";

        return RedirectToAction("Edit", "Products", new { area = "Admin", id = productId });
    }

    private async Task<string> GenerateUniqueSlugAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return $"media-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));
        var baseSlug = string.IsNullOrWhiteSpace(slug) ? "media" : slug;
        var uniqueSlug = $"{baseSlug}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var suffix = 1;

        while (await _dbContext.Media.AsNoTracking().AnyAsync(m => m.Slug == uniqueSlug))
        {
            uniqueSlug = $"{baseSlug}-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
            suffix++;
        }

        return uniqueSlug;
    }

    private static (IImageEncoder Encoder, string MimeType) ResolveEncoder(string? format, IImageFormat detectedFormat)
    {
        var normalized = (format ?? detectedFormat.DefaultMimeType)?.ToLowerInvariant();
        return normalized switch
        {
            "webp" or "image/webp" => (new WebpEncoder { Quality = 82 }, "image/webp"),
            "jpg" or "jpeg" or "image/jpg" or "image/jpeg" => (new JpegEncoder { Quality = 85 }, "image/jpeg"),
            _ => detectedFormat.Name.Equals("JPEG", StringComparison.OrdinalIgnoreCase)
                ? (new JpegEncoder { Quality = 85 }, "image/jpeg")
                : (new WebpEncoder { Quality = 82 }, "image/webp")
        };
    }
}
