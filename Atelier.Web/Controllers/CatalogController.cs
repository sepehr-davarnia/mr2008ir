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
        "admin"
    };

    public CatalogController(AtelierDbContext dbContext) : base(dbContext)
    {
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Index()
    {
        var categories = await LoadCategoryNodesAsync();
        var (mediaMap, fallbackMedia) = await LoadCategoryMediaAsync(categories);
        var tree = BuildCategoryTree(categories, null, mediaMap, fallbackMedia);

        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "خانه", Url = Url.Action("Index", "Home") },
            new() { Title = "دسته بندی ها" }
        };

        var model = new CategoryViewModel
        {
            Categories = tree,
            Breadcrumbs = breadcrumbs
        };

        var canonicalUrl = Url.Action("Index", "Catalog", new { }, Request.Scheme);
        model.MetaTitle = "دسته بندی محصولات چوب و ترمووود";
        model.MetaDescription = "دسته بندی های تخصصی چوب طبیعی، ترمووود و کاربردهای معماری.";
        model.CanonicalUrl = canonicalUrl ?? string.Empty;
        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl);
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.WebPage,
            Title = "دسته بندی محصولات چوب و ترمووود",
            Description = "دسته بندی های تخصصی چوب طبیعی، ترمووود و کاربردهای معماری.",
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
                var product = await DbContext.Products
                    .AsNoTracking()
                    .Where(item => item.Slug == productSlug && item.Status == ProductStatus.Published)
                    .Select(item => new ProductSnapshot
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Slug = item.Slug,
                        Description = item.Description,
                        Price = item.Price,
                        PriceType = item.PriceType
                    })
                    .FirstOrDefaultAsync();

                if (product is not null && IsProductInCategory(productSlug, categoryChain.Last().Slug, categories))
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

        var productSlugs = CatalogRoutingHelper.GetProductSlugsForCategory(currentCategory.Slug, categories);
        var productData = productSlugs.Count == 0
            ? new List<ProductSnapshot>()
            : await DbContext.Products
                .AsNoTracking()
                .Where(product => productSlugs.Contains(product.Slug) && product.Status == ProductStatus.Published)
                .OrderBy(product => product.Name)
                .Select(product => new ProductSnapshot
                {
                    Id = product.Id,
                    Name = product.Name,
                    Slug = product.Slug,
                    Description = product.Description,
                    Price = product.Price,
                    PrimaryMedia = product.Gallery
                        .OrderBy(media => media.Id)
                        .Select(media => new MediaSnapshot(media.Url, media.AltText))
                        .FirstOrDefault()
                })
                .ToListAsync();

        var products = productData.Select(product => new ProductCardViewModel
        {
            Name = product.Name,
            Url = BuildProductUrl(categoryChain, product.Slug),
            ShortDescription = SeoContentHelper.BuildShortDescription(SeoContentHelper.ExtractDescription(product.Description)),
            ImageUrl = product.PrimaryMedia?.Url,
            ImageAltText = product.PrimaryMedia?.AltText ?? product.Name,
            PriceDisplay = LocalizationHelper.FormatPrice(product.Price, product.PriceType)
        }).ToList();

        var breadcrumbs = BuildBreadcrumbs(categoryChain);

        var model = new ProductListViewModel
        {
            Title = currentCategory.Name,
            Description = $"محصولات تخصصی دسته {currentCategory.Name} برای پروژه های چوب و ترمووود.",
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
            Title = product.Name,
            Description = description,
            ShortDescription = SeoContentHelper.BuildShortDescription(description),
            PriceDisplay = LocalizationHelper.FormatPrice(product.Price, product.PriceType),
            HeroImageUrl = heroMedia?.Url,
            HeroImageAltText = heroMedia?.AltText ?? product.Name,
            Breadcrumbs = breadcrumbs,
            Specifications = specs,
            Gallery = gallery
        };

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
            Price = product.Price,
            CurrencyCode = "IRR"
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

    private static bool IsProductInCategory(string productSlug, string categorySlug, IReadOnlyList<CatalogCategoryNode> categories)
    {
        return CatalogRoutingHelper.GetProductSlugsForCategory(categorySlug, categories).Contains(productSlug);
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
