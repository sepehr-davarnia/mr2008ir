using Atelier.Infrastructure.Data;
using Atelier.Domain.Enums;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

public class CatalogController : PublicControllerBase
{
    private static readonly HashSet<string> ReservedSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "blog",
        "pages",
        "projects",
        "categories",
        "cart",
        "checkout",
        "payment",
        "order-tracking",
        "admin"
    };

    public CatalogController(AtelierDbContext dbContext) : base(dbContext)
    {
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Index(string? q, int? year, string? engine, string? trim)
    {
        var categories = await LoadCategoryNodesAsync();
        var (mediaMap, fallbackMedia) = await LoadCategoryMediaAsync(categories);
        var tree = BuildCategoryTree(categories, null, mediaMap, fallbackMedia);

        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "خانه", Url = Url.Action("Index", "Home") },
            new() { Title = "دسته بندی ها" }
        };

        var normalizedQuery = string.IsNullOrWhiteSpace(q) ? null : PersianSearchNormalizer.Normalize(q);
        var query = normalizedQuery?[..Math.Min(normalizedQuery.Length, 100)];
        var searchResults = new List<ProductCardViewModel>();
        engine = string.IsNullOrWhiteSpace(engine) ? null : engine.Trim();
        trim = string.IsNullOrWhiteSpace(trim) ? null : trim.Trim();
        var hasVehicleFilter = year.HasValue || engine is not null || trim is not null;
        var hasDiscoveryFilter = query is not null || hasVehicleFilter;
        if (hasDiscoveryFilter)
        {
            var compactQuery = PersianSearchNormalizer.CompactPartNumber(query ?? string.Empty);
            var matches = await DbContext.Products.AsNoTracking()
                .Where(product => product.Status == ProductStatus.Published &&
                    (query == null || product.Name.Contains(query) || product.Slug.Contains(query) ||
                     (product.Description != null && product.Description.Contains(query)) ||
                     (product.Brand != null && product.Brand.Contains(query)) ||
                     (product.Manufacturer != null && product.Manufacturer.Contains(query)) ||
                     (product.OemPartNumber != null && (product.OemPartNumber.Contains(query) || (compactQuery.Length >= 2 && product.OemPartNumber.Replace("-", "").Replace(" ", "").Contains(compactQuery)))) ||
                     (product.TechnicalPartNumber != null && (product.TechnicalPartNumber.Contains(query) || (compactQuery.Length >= 2 && product.TechnicalPartNumber.Replace("-", "").Replace(" ", "").Contains(compactQuery)))) ||
                     (product.AlternatePartNumbers != null && product.AlternatePartNumbers.Contains(query))) &&
                    (!hasVehicleFilter || product.Compatibilities.Any(c => (!year.HasValue || (c.Vehicle.YearFrom <= year.Value && (c.Vehicle.YearTo == null || c.Vehicle.YearTo >= year.Value))) &&
                                                     (engine == null || c.Vehicle.Engine == engine) &&
                                                     (trim == null || c.Vehicle.Trim == trim))))
                .OrderBy(product => product.Name).Take(30)
                .Select(product => new ProductSnapshot
                {
                    Id = product.Id, Name = product.Name, Slug = product.Slug,
                    Description = product.Description, Price = product.Price, PriceType = product.PriceType,
                    PrimaryMedia = product.Gallery.OrderBy(media => media.Id)
                        .Select(media => new MediaSnapshot(media.Url, media.AltText)).FirstOrDefault(),
                    PrimaryCategoryId = product.Categories.OrderBy(category => category.Id).Select(category => (int?)category.Id).FirstOrDefault()
                }).ToListAsync();

            foreach (var product in matches)
            {
                var category = categories.FirstOrDefault(item => item.Id == product.PrimaryCategoryId);
                if (category is null) continue;
                var chain = CatalogRoutingHelper.BuildCategoryChain(categories, category);
                searchResults.Add(new ProductCardViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Url = CatalogRoutingHelper.BuildProductPath(chain, product.Slug),
                    ShortDescription = SeoContentHelper.BuildShortDescription(SeoContentHelper.ExtractDescription(product.Description)),
                    ImageUrl = product.PrimaryMedia?.Url,
                    ImageAltText = product.PrimaryMedia?.AltText ?? product.Name,
                    PriceDisplay = LocalizationHelper.FormatPrice(product.Price, product.PriceType),
                    CanPurchaseOnline = product.PriceType == PriceType.Fixed && product.Price > 0
                });
            }
        }

        var yearRanges = await DbContext.Vehicles.AsNoTracking().Where(v => v.IsActive)
            .Select(v => new { v.YearFrom, v.YearTo }).ToListAsync();
        var yearOptions = yearRanges.SelectMany(v => Enumerable.Range(v.YearFrom, (v.YearTo ?? v.YearFrom) - v.YearFrom + 1))
            .Distinct().OrderByDescending(value => value).ToList();

        var model = new CategoryViewModel
        {
            Categories = tree,
            Breadcrumbs = breadcrumbs,
            Query = query,
            VehicleYear = year,
            Engine = engine,
            Trim = trim,
            YearOptions = yearOptions,
            EngineOptions = await DbContext.Vehicles.AsNoTracking().Where(v => v.IsActive).Select(v => v.Engine).Distinct().OrderBy(value => value).ToListAsync(),
            TrimOptions = await DbContext.Vehicles.AsNoTracking().Where(v => v.IsActive).Select(v => v.Trim).Distinct().OrderBy(value => value).ToListAsync(),
            SearchResults = searchResults
        };

        var canonicalUrl = Url.Action("Index", "Catalog", new { }, Request.Scheme);
        model.MetaTitle = "دسته‌بندی قطعات پژو ۲۰۰۸ | mr2008.ir";
        model.MetaDescription = "خرید قطعات موتور، ترمز، تعلیق، بدنه، برق و لوازم مصرفی پژو ۲۰۰۸.";
        model.CanonicalUrl = canonicalUrl ?? string.Empty;
        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl);
        if (hasDiscoveryFilter) ViewData["Robots"] = "noindex,follow";
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.WebPage,
            Title = "دسته‌بندی قطعات پژو ۲۰۰۸ | mr2008.ir",
            Description = "خرید قطعات موتور، ترمز، تعلیق، بدنه، برق و لوازم مصرفی پژو ۲۰۰۸.",
            CanonicalUrl = canonicalUrl ?? string.Empty
        });

        return View(model);
    }

    [HttpGet("{**categoryPath}", Order = 999)]
    public async Task<IActionResult> ProductOrCategory(string? categoryPath)
    {
        if (string.IsNullOrWhiteSpace(categoryPath))
        {
            return NotFound();
        }

        var segments = categoryPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (segments.Count == 0 || ReservedSegments.Contains(segments[0]))
        {
            return NotFound();
        }

        var categories = await LoadCategoryNodesAsync();

        if (segments.Count >= 2)
        {
            var categorySegments = segments.Take(segments.Count - 1).ToList();
            if (TryResolveCategoryPath(categories, categorySegments, out var categoryChain))
            {
                var productSlug = segments.Last();
                var categoryId = categoryChain.Last().Id;
                var product = await DbContext.Products
                    .AsNoTracking()
                    .Where(item => item.Slug == productSlug && item.Status == ProductStatus.Published &&
                                   item.Categories.Any(category => category.Id == categoryId))
                    .Select(item => new ProductSnapshot
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Slug = item.Slug,
                        Description = item.Description,
                        Price = item.Price,
                        PriceType = item.PriceType,
                        Brand = item.Brand,
                        Manufacturer = item.Manufacturer,
                        OemPartNumber = item.OemPartNumber,
                        TechnicalPartNumber = item.TechnicalPartNumber,
                        AlternatePartNumbers = item.AlternatePartNumbers
                    })
                    .FirstOrDefaultAsync();

                if (product is not null)
                {
                    return await RenderProductDetailAsync(product, categoryChain);
                }
            }
        }

        if (!TryResolveCategoryPath(categories, segments, out var matchedChain))
        {
            return NotFound();
        }

        return await RenderCategoryListingAsync(categories, matchedChain);
    }

    private async Task<IActionResult> RenderCategoryListingAsync(IReadOnlyList<CatalogCategoryNode> categories, IReadOnlyList<CatalogCategoryNode> categoryChain)
    {
        var currentCategory = categoryChain.Last();
        var (mediaMap, fallbackMedia) = await LoadCategoryMediaAsync(categories);
        var heroMedia = currentCategory.MediaId.HasValue && mediaMap.TryGetValue(currentCategory.MediaId.Value, out var categoryMedia)
            ? categoryMedia
            : fallbackMedia;
        var subcategories = categories
            .Where(item => item.ParentId == currentCategory.Id)
            .Select(item => BuildCategoryCard(item, categoryChain, mediaMap, fallbackMedia))
            .ToList();

        var categoryIds = GetCategoryAndDescendantIds(currentCategory.Id, categories);
        var productData = await DbContext.Products
                .AsNoTracking()
                .Where(product => product.Status == ProductStatus.Published &&
                                  product.Categories.Any(category => categoryIds.Contains(category.Id)))
                .OrderBy(product => product.Name)
                .Select(product => new ProductSnapshot
                {
                    Id = product.Id,
                    Name = product.Name,
                    Slug = product.Slug,
                    Description = product.Description,
                    Price = product.Price,
                    PriceType = product.PriceType,
                    PrimaryMedia = product.Gallery
                        .OrderBy(media => media.Id)
                        .Select(media => new MediaSnapshot(media.Url, media.AltText))
                        .FirstOrDefault()
                })
                .ToListAsync();

        var products = productData.Select(product => new ProductCardViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Url = BuildProductUrl(categoryChain, product.Slug),
            ShortDescription = SeoContentHelper.BuildShortDescription(SeoContentHelper.ExtractDescription(product.Description)),
            ImageUrl = product.PrimaryMedia?.Url,
            ImageAltText = product.PrimaryMedia?.AltText ?? product.Name,
            PriceDisplay = LocalizationHelper.FormatPrice(product.Price, product.PriceType),
            CanPurchaseOnline = product.PriceType == PriceType.Fixed && product.Price > 0
        }).ToList();

        var breadcrumbs = BuildBreadcrumbs(categoryChain);

        var model = new ProductListViewModel
        {
            Title = currentCategory.Name,
            Description = $"قطعات تخصصی {currentCategory.Name} مناسب پژو ۲۰۰۸ با بررسی فنی پیش از خرید.",
            HeroMediaUrl = heroMedia?.Url,
            HeroMediaAltText = heroMedia?.AltText ?? currentCategory.Name,
            Breadcrumbs = breadcrumbs,
            Subcategories = subcategories,
            Products = products
        };

        var canonicalUrl = SeoHelper.BuildAbsoluteUrl(Request, BuildCategoryPath(categoryChain));
        model.MetaTitle = currentCategory.Name;
        model.MetaDescription = model.Description;
        model.CanonicalUrl = canonicalUrl;
        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl);
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.WebPage,
            Title = currentCategory.Name,
            Description = model.Description,
            CanonicalUrl = canonicalUrl
        });

        return View("Category", model);
    }

    private async Task<IActionResult> RenderProductDetailAsync(ProductSnapshot product, IReadOnlyList<CatalogCategoryNode> categoryChain)
    {
        var breadcrumbs = BuildBreadcrumbs(categoryChain);
        breadcrumbs.Add(new BreadcrumbItemViewModel { Title = product.Name });

        var gallery = await DbContext.Media
            .AsNoTracking()
            .Where(media => EF.Property<int?>(media, "ProductId") == product.Id)
            .Select(media => new MediaItemViewModel
            {
                Url = media.Url,
                AltText = media.AltText
            })
            .ToListAsync();

        var specs = SeoContentHelper.ParseSpecifications(product.Description);
        var description = SeoContentHelper.ExtractDescription(product.Description);
        var canonicalUrl = SeoHelper.BuildAbsoluteUrl(Request, BuildProductPath(categoryChain, product.Slug));
        var heroMedia = gallery.FirstOrDefault();

        var model = new ProductDetailViewModel
        {
            Id = product.Id,
            Title = product.Name,
            Description = description,
            ShortDescription = SeoContentHelper.BuildShortDescription(description),
            PriceDisplay = LocalizationHelper.FormatPrice(product.Price, product.PriceType),
            CanPurchaseOnline = product.PriceType == PriceType.Fixed && product.Price > 0,
            HeroImageUrl = heroMedia?.Url,
            HeroImageAltText = heroMedia?.AltText ?? product.Name,
            Breadcrumbs = breadcrumbs,
            Specifications = specs,
            Gallery = gallery
        };

        model.Brand = product.Brand;
        model.Manufacturer = product.Manufacturer;
        model.OemPartNumber = product.OemPartNumber;
        model.TechnicalPartNumber = product.TechnicalPartNumber;
        model.AlternatePartNumbers = product.AlternatePartNumbers;
        model.CompatibleVehicles = await DbContext.ProductCompatibilities.AsNoTracking()
            .Where(item => item.ProductId == product.Id && item.Vehicle.IsActive)
            .OrderBy(item => item.Vehicle.Make).ThenBy(item => item.Vehicle.Model).ThenBy(item => item.Vehicle.YearFrom)
            .Select(item => new VehicleCompatibilityViewModel
            {
                Make = item.Vehicle.Make, Model = item.Vehicle.Model, YearFrom = item.Vehicle.YearFrom,
                YearTo = item.Vehicle.YearTo, Engine = item.Vehicle.Engine, Trim = item.Vehicle.Trim,
                RequiresVinCheck = item.RequiresVinCheck, Note = item.Note
            }).ToListAsync();

        model.MetaTitle = product.Name;
        model.MetaDescription = SeoContentHelper.BuildMetaDescription(description) ?? product.Name;
        model.CanonicalUrl = canonicalUrl;
        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl, gallery.FirstOrDefault()?.Url, "product");
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["ProductSchema"] = SeoHelper.BuildProductSchema(new SeoProductSchemaData
        {
            Name = product.Name,
            Description = SeoContentHelper.BuildMetaDescription(description),
            CanonicalUrl = canonicalUrl,
            ImageUrl = gallery.FirstOrDefault()?.Url,
            Price = product.Price.HasValue ? product.Price.Value * 10 : null,
            CurrencyCode = "IRR",
            Sku = product.TechnicalPartNumber ?? product.Slug,
            Mpn = product.OemPartNumber,
            Brand = product.Brand,
            AvailabilityUrl = "https://schema.org/InStock",
            ItemConditionUrl = "https://schema.org/NewCondition",
            ReturnPolicyUrl = SeoHelper.BuildAbsoluteUrl(Request, "/pages/shipping-returns")
        });

        return View("Product", model);
    }

    private sealed class ProductSnapshot
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal? Price { get; init; }
        public PriceType PriceType { get; init; }
        public MediaSnapshot? PrimaryMedia { get; init; }
        public int? PrimaryCategoryId { get; init; }
        public string? Brand { get; init; }
        public string? Manufacturer { get; init; }
        public string? OemPartNumber { get; init; }
        public string? TechnicalPartNumber { get; init; }
        public string? AlternatePartNumbers { get; init; }
    }

    private async Task<IReadOnlyList<CatalogCategoryNode>> LoadCategoryNodesAsync()
    {
        return await DbContext.Categories
            .AsNoTracking()
            .Select(category => new CatalogCategoryNode
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ParentId = EF.Property<int?>(category, "ParentId"),
                MediaId = category.MediaId
            })
            .ToListAsync();
    }

    private static string BuildCategorySummary(string categoryName)
    {
        return $"مرور محصولات {categoryName} و کاربردهای تخصصی آن.";
    }

    private static IReadOnlyList<CategoryTreeItemViewModel> BuildCategoryTree(
        IReadOnlyList<CatalogCategoryNode> categories,
        int? parentId,
        IReadOnlyDictionary<int, MediaSnapshot> mediaMap,
        MediaSnapshot? fallbackMedia)
    {
        return categories
            .Where(category => category.ParentId == parentId)
            .OrderBy(category => category.Name)
            .Select(category =>
            {
                var media = category.MediaId.HasValue && mediaMap.TryGetValue(category.MediaId.Value, out var categoryMedia)
                    ? categoryMedia
                    : fallbackMedia;

                return new CategoryTreeItemViewModel
                {
                    Name = category.Name,
                    Url = BuildCategoryPath(categories, category),
                    ImageUrl = media?.Url,
                    ImageAltText = media?.AltText ?? category.Name,
                    Summary = BuildCategorySummary(category.Name),
                    Children = BuildCategoryTree(categories, category.Id, mediaMap, fallbackMedia)
                };
            })
            .ToList();
    }

    private static string BuildCategoryPath(IReadOnlyList<CatalogCategoryNode> categories, CatalogCategoryNode category)
    {
        var chain = CatalogRoutingHelper.BuildCategoryChain(categories, category);
        return CatalogRoutingHelper.BuildCategoryPath(chain);
    }

    private static string BuildCategoryPath(IReadOnlyList<CatalogCategoryNode> categoryChain)
    {
        return CatalogRoutingHelper.BuildCategoryPath(categoryChain);
    }

    private static string BuildProductPath(IReadOnlyList<CatalogCategoryNode> categoryChain, string productSlug)
    {
        return CatalogRoutingHelper.BuildProductPath(categoryChain, productSlug);
    }

    private static string BuildCategoryUrl(IReadOnlyList<CatalogCategoryNode> categoryChain, CatalogCategoryNode category)
    {
        var chain = categoryChain.ToList();
        chain.Add(category);
        return BuildCategoryPath(chain);
    }

    private static string BuildProductUrl(IReadOnlyList<CatalogCategoryNode> categoryChain, string productSlug)
    {
        return BuildProductPath(categoryChain, productSlug);
    }

    private static bool TryResolveCategoryPath(
        IReadOnlyList<CatalogCategoryNode> categories,
        IReadOnlyList<string> segments,
        out List<CatalogCategoryNode> chain)
    {
        chain = new List<CatalogCategoryNode>();
        int? parentId = null;
        string? parentSlug = null;

        foreach (var segment in segments)
        {
            var matched = categories.FirstOrDefault(category =>
                category.ParentId == parentId &&
                CatalogRoutingHelper.IsSegmentMatch(category.Slug, segment, parentSlug));

            if (matched is null)
            {
                chain.Clear();
                return false;
            }

            chain.Add(matched);
            parentId = matched.Id;
            parentSlug = matched.Slug;
        }

        return chain.Count > 0;
    }

    private static HashSet<int> GetCategoryAndDescendantIds(int categoryId, IReadOnlyList<CatalogCategoryNode> categories)
    {
        var result = new HashSet<int> { categoryId };
        var queue = new Queue<int>();
        queue.Enqueue(categoryId);
        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();
            foreach (var child in categories.Where(item => item.ParentId == parentId))
                if (result.Add(child.Id)) queue.Enqueue(child.Id);
        }
        return result;
    }

    private static List<BreadcrumbItemViewModel> BuildBreadcrumbs(IReadOnlyList<CatalogCategoryNode> chain)
    {
        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "خانه", Url = "/" }
        };

        string? parentSlug = null;
        var pathSegments = new List<string>();

        foreach (var node in chain)
        {
            var segment = CatalogRoutingHelper.GetSegment(node.Slug, parentSlug);
            pathSegments.Add(segment);
            breadcrumbs.Add(new BreadcrumbItemViewModel
            {
                Title = node.Name,
                Url = "/" + string.Join('/', pathSegments)
            });
            parentSlug = node.Slug;
        }

        return breadcrumbs;
    }

    private async Task<(IReadOnlyDictionary<int, MediaSnapshot> MediaMap, MediaSnapshot? FallbackMedia)> LoadCategoryMediaAsync(
        IReadOnlyList<CatalogCategoryNode> categories)
    {
        var settings = await DbContext.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
        var mediaIds = categories
            .Where(category => category.MediaId.HasValue)
            .Select(category => category.MediaId!.Value)
            .ToHashSet();

        if (settings?.DefaultCategoryMediaId is not null)
        {
            mediaIds.Add(settings.DefaultCategoryMediaId.Value);
        }

        var mediaMap = mediaIds.Count == 0
            ? new Dictionary<int, MediaSnapshot>()
            : await DbContext.Media
                .AsNoTracking()
                .Where(media => mediaIds.Contains(media.Id))
                .ToDictionaryAsync(media => media.Id, media => new MediaSnapshot(media.Url, media.AltText));

        var fallbackMedia = settings?.DefaultCategoryMediaId is not null
            && mediaMap.TryGetValue(settings.DefaultCategoryMediaId.Value, out var fallback)
            ? fallback
            : null;

        return (mediaMap, fallbackMedia);
    }

    private static CategoryCardViewModel BuildCategoryCard(
        CatalogCategoryNode category,
        IReadOnlyList<CatalogCategoryNode> categoryChain,
        IReadOnlyDictionary<int, MediaSnapshot> mediaMap,
        MediaSnapshot? fallbackMedia)
    {
        var media = category.MediaId.HasValue && mediaMap.TryGetValue(category.MediaId.Value, out var categoryMedia)
            ? categoryMedia
            : fallbackMedia;

        return new CategoryCardViewModel
        {
            Name = category.Name,
            Url = BuildCategoryUrl(categoryChain, category),
            ImageUrl = media?.Url,
            ImageAltText = media?.AltText ?? category.Name,
            Summary = BuildCategorySummary(category.Name)
        };
    }

    private sealed record MediaSnapshot(string Url, string? AltText);
}
