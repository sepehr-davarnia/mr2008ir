using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Infrastructure.Data.Entities;
using Atelier.Web.Areas.Admin.Services;
using Atelier.Web.Areas.Admin.ViewModels;
using Atelier.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ProductsController : Controller
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf"
    ];

    private readonly AtelierDbContext _dbContext;

    public ProductsController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var query = _dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product => product.Name.Contains(search) || product.Slug.Contains(search));
        }

        var products = await query
            .OrderByDescending(product => product.CreatedAt)
            .Select(product => new ProductListItemViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Status = product.Status
            })
            .ToListAsync();

        var model = new ProductIndexViewModel
        {
            Search = search,
            Products = products
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ProductCreateViewModel();
        await PopulateProductOptionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateProductOptionsAsync(model);
            return View(model);
        }

        var slug = await EnsureUniqueProductSlugAsync(model.Slug, model.Name, null);
        if (slug is null)
        {
            ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
            await PopulateProductOptionsAsync(model);
            return View(model);
        }

        var product = new Product(model.Name, slug, model.Description);
        product.UpdateCommerceDetails(model.Brand, model.Manufacturer, model.OemPartNumber, model.TechnicalPartNumber, model.AlternatePartNumbers);
        product.SetCategories(await _dbContext.Categories.Where(item => model.CategoryIds.Contains(item.Id)).ToListAsync());
        product.SetCompatibilities(await BuildCompatibilitiesAsync(model.VehicleIds, model.RequiresVinCheck));
        product.SetPrice(model.Price);
        ApplyStatus(product, model.Status);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "محصول با موفقیت ذخیره شد.";

        return RedirectToAction("Edit", new { id = product.Id, area = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await BuildEditViewModelAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductEditViewModel model)
    {
        var product = await _dbContext.Products
            .Include(p => p.Gallery)
            .Include(p => p.Categories)
            .Include(p => p.Compatibilities)
            .FirstOrDefaultAsync(p => p.Id == model.Id);

        if (product is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.Gallery = product.Gallery
                .Select(media => new MediaItemViewModel
                {
                    Id = media.Id,
                    Title = media.Title,
                    AltText = media.AltText,
                    ContentType = media.ContentType
                })
                .ToList();
            await PopulateProductOptionsAsync(model);
            return View(model);
        }

        var slug = await EnsureUniqueProductSlugAsync(model.Slug, model.Name, product.Id);
        if (slug is null)
        {
            ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
            model.Gallery = product.Gallery
                .Select(media => new MediaItemViewModel
                {
                    Id = media.Id,
                    Title = media.Title,
                    AltText = media.AltText,
                    ContentType = media.ContentType
                })
                .ToList();
            await PopulateProductOptionsAsync(model);
            return View(model);
        }

        product.UpdateDetails(model.Name, model.Description);
        product.UpdateCommerceDetails(model.Brand, model.Manufacturer, model.OemPartNumber, model.TechnicalPartNumber, model.AlternatePartNumbers);
        product.SetCategories(await _dbContext.Categories.Where(item => model.CategoryIds.Contains(item.Id)).ToListAsync());
        product.SetCompatibilities(await BuildCompatibilitiesAsync(model.VehicleIds, model.RequiresVinCheck));
        product.SetPrice(model.Price);
        ApplyStatus(product, model.Status);

        _dbContext.Entry(product).Property("Slug").CurrentValue = slug;
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "تغییرات محصول ذخیره شد.";

        return RedirectToAction("Edit", new { id = product.Id, area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _dbContext.Products
            .Include(p => p.Gallery)
            .Include(p => p.Categories)
            .Include(p => p.Compatibilities)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "محصول با موفقیت حذف شد.";

        return RedirectToAction("Index", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMedia(int productId, IFormFile? file, string? title, string? altText)
    {
        var product = await _dbContext.Products
            .Include(p => p.Gallery)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
        {
            return NotFound();
        }

        var validationMessage = await ValidateFileAsync(file);
        if (validationMessage is not null)
        {
            TempData["ErrorMessage"] = validationMessage;
            return RedirectToAction("Edit", new { id = productId, area = "Admin" });
        }

        if (MediaAltTextHelper.IsImageContentType(file!.ContentType) && !MediaAltTextHelper.HasAltText(altText))
        {
            TempData["ErrorMessage"] = MediaAltTextHelper.RequiredMessage;
            return RedirectToAction("Edit", new { id = productId, area = "Admin" });
        }

        await using var memoryStream = new MemoryStream();
        await file!.CopyToAsync(memoryStream);

        var mediaContent = new MediaContent
        {
            Data = memoryStream.ToArray()
        };

        _dbContext.MediaContents.Add(mediaContent);
        await _dbContext.SaveChangesAsync();

        var slug = await GenerateUniqueMediaSlugAsync(Path.GetFileNameWithoutExtension(file.FileName));

        var media = new Media(
            slug,
            string.Empty,
            mediaContent.Id,
            file.ContentType,
            file.Length,
            title,
            altText,
            file.FileName,
            "Product");

        product.AddMedia(media);
        _dbContext.Media.Add(media);
        await _dbContext.SaveChangesAsync();

        var url = Url.Action("Serve", "Media", new { area = "Admin", mediaId = media.Id }) ?? string.Empty;
        media.UpdateUrl(url);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "تصویر با موفقیت ذخیره شد.";

        return RedirectToAction("Edit", new { id = productId, area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMediaFromLibrary(int productId, int? mediaId)
    {
        if (!mediaId.HasValue)
        {
            TempData["ErrorMessage"] = "لطفاً یک تصویر انتخاب کنید.";
            return RedirectToAction("Edit", new { id = productId, area = "Admin" });
        }

        var product = await _dbContext.Products
            .Include(p => p.Gallery)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null)
        {
            return NotFound();
        }

        var media = await _dbContext.Media.FirstOrDefaultAsync(item => item.Id == mediaId.Value);
        if (media is null)
        {
            TempData["ErrorMessage"] = "تصویر انتخاب شده پیدا نشد.";
            return RedirectToAction("Edit", new { id = productId, area = "Admin" });
        }

        if (MediaAltTextHelper.IsImageContentType(media.ContentType) && !MediaAltTextHelper.HasAltText(media.AltText))
        {
            TempData["ErrorMessage"] = MediaAltTextHelper.RequiredMessage;
            return RedirectToAction("Edit", new { id = productId, area = "Admin" });
        }

        var currentProductId = _dbContext.Entry(media).Property<int?>("ProductId").CurrentValue;
        if (currentProductId.HasValue && currentProductId.Value != productId)
        {
            TempData["ErrorMessage"] = "این تصویر به محصول دیگری اختصاص دارد.";
            return RedirectToAction("Edit", new { id = productId, area = "Admin" });
        }

        if (product.Gallery.Any(item => item.Id == media.Id))
        {
            TempData["ErrorMessage"] = "این تصویر قبلاً به این محصول اضافه شده است.";
            return RedirectToAction("Edit", new { id = productId, area = "Admin" });
        }

        product.AddMedia(media);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "تصویر با موفقیت ذخیره شد.";
        return RedirectToAction("Edit", new { id = productId, area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMedia(int productId, int mediaId)
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

        return RedirectToAction("Edit", new { id = productId, area = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> Pricing()
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .Select(product => new ProductPriceItemViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Status = product.Status,
                Price = product.Price,
                PriceType = product.PriceType,
                PriceDisplay = LocalizationHelper.FormatPrice(product.Price, product.PriceType)
            })
            .ToListAsync();

        var model = new ProductPricingIndexViewModel
        {
            Products = products
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePrice(ProductPriceUpdateViewModel model)
    {
        if (!model.IsContactPrice && !ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "لطفاً مبلغ معتبر وارد کنید.";
            return RedirectToAction("Pricing");
        }

        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == model.Id);

        if (product is null)
        {
            return NotFound();
        }

        if (model.IsContactPrice)
        {
            product.SetPrice(null);
        }
        else
        {
            if (model.Price is null)
            {
                TempData["ErrorMessage"] = "لطفاً مبلغ معتبر وارد کنید.";
                return RedirectToAction("Pricing");
            }

            product.SetPrice(model.Price);
        }
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "قیمت محصول به‌روزرسانی شد.";
        return RedirectToAction("Pricing");
    }

    private async Task<ProductEditViewModel?> BuildEditViewModelAsync(int id)
    {
        var product = await _dbContext.Products
            .Include(p => p.Gallery)
            .Include(p => p.Categories)
            .Include(p => p.Compatibilities)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return null;
        }

        var model = new ProductEditViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Status = product.Status,
            Price = product.Price,
            Brand = product.Brand,
            Manufacturer = product.Manufacturer,
            OemPartNumber = product.OemPartNumber,
            TechnicalPartNumber = product.TechnicalPartNumber,
            AlternatePartNumbers = product.AlternatePartNumbers,
            CategoryIds = product.Categories.Select(item => item.Id).ToList(),
            VehicleIds = product.Compatibilities.Select(item => item.VehicleId).ToList(),
            RequiresVinCheck = product.Compatibilities.Any(item => item.RequiresVinCheck),
            Gallery = product.Gallery
                .Select(media => new MediaItemViewModel
                {
                    Id = media.Id,
                    Title = media.Title,
                    AltText = media.AltText,
                    ContentType = media.ContentType
                })
                .ToList()
        };

        await PopulateProductOptionsAsync(model);
        return model;
    }

    private async Task PopulateProductOptionsAsync(ProductFormViewModel model)
    {
        model.CategoryOptions = await _dbContext.Categories.AsNoTracking().OrderBy(item => item.Name)
            .Select(item => new SelectOptionViewModel { Id = item.Id, Label = item.Name }).ToListAsync();
        model.VehicleOptions = await _dbContext.Vehicles.AsNoTracking().Where(item => item.IsActive)
            .OrderBy(item => item.Make).ThenBy(item => item.Model).ThenBy(item => item.YearFrom)
            .Select(item => new SelectOptionViewModel
            {
                Id = item.Id,
                Label = item.Make + " " + item.Model + " | " + item.YearFrom + "-" + (item.YearTo ?? item.YearFrom) + " | " + item.Engine + " | " + item.Trim
            }).ToListAsync();
    }

    private async Task<List<ProductCompatibility>> BuildCompatibilitiesAsync(IEnumerable<int> vehicleIds, bool requiresVinCheck)
    {
        var ids = vehicleIds.Distinct().ToArray();
        var validIds = await _dbContext.Vehicles.AsNoTracking().Where(item => ids.Contains(item.Id) && item.IsActive)
            .Select(item => item.Id).ToListAsync();
        return validIds.Select(id => new ProductCompatibility(id, requiresVinCheck,
            requiresVinCheck ? "تطبیق با VIN پیش از ارسال الزامی است." : null)).ToList();
    }

    private static void ApplyStatus(Product product, ProductStatus status)
    {
        switch (status)
        {
            case ProductStatus.Published:
                product.Publish();
                break;
            case ProductStatus.Archived:
                product.Archive();
                break;
            default:
                product.MarkDraft();
                break;
        }
    }

    private async Task<string?> ValidateFileAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return "لطفاً یک فایل معتبر انتخاب کنید.";
        }

        var settings = await UploadSettingsHelper.GetOrCreateSettingsAsync(_dbContext);
        var maxUploadBytes = UploadSettingsHelper.ToBytes(settings.MaxUploadSizeKb);
        if (file.Length > maxUploadBytes)
        {
            var display = UploadSettingsHelper.FormatMegabytes(settings.MaxUploadSizeKb);
            return $"حجم فایل نباید بیشتر از {display} مگابایت باشد.";
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return "فرمت فایل باید تصویر یا PDF باشد.";
        }

        return null;
    }

    private async Task<string?> EnsureUniqueProductSlugAsync(string? requestedSlug, string name, int? productId)
    {
        var slugSource = string.IsNullOrWhiteSpace(requestedSlug) ? name : requestedSlug;
        var slug = GenerateSlug(slugSource);

        var isDuplicate = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(product => product.Slug == slug && product.Id != productId);

        return isDuplicate ? null : slug;
    }

    private async Task<string> GenerateUniqueMediaSlugAsync(string input)
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
            return $"item-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }

        var normalized = input.Trim().ToLowerInvariant();
        var chars = normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        var slug = new string(chars);
        slug = string.Join('-', slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(slug)
            ? $"item-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : slug;
    }
}
