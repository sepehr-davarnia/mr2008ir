using Atelier.Infrastructure.Data;
using Atelier.Domain.Enums;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

public class HomeController : PublicControllerBase
{
    public HomeController(AtelierDbContext dbContext) : base(dbContext)
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var settings = await DbContext.SiteSettings
            .AsNoTracking()
            .FirstOrDefaultAsync();

        var topCategoryData = await DbContext.Categories
            .AsNoTracking()
            .Where(category => EF.Property<int?>(category, "ParentId") == null)
            .OrderBy(category => category.Name)
            .Select(category => new
            {
                category.Name,
                category.Slug,
                category.MediaId
            })
            .ToListAsync();

        var categoryNodes = await DbContext.Categories
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

        var productData = await DbContext.Products
            .AsNoTracking()
            .Where(product => product.Status == ProductStatus.Published)
            .OrderByDescending(product => product.CreatedAt)
            .Take(8)
            .Select(product => new
            {
                product.Id,
                product.Name,
                product.Slug,
                product.Description,
                product.Price,
                product.PriceType,
                PrimaryMedia = product.Gallery
                    .OrderBy(media => media.Id)
                    .Select(media => new MediaSnapshot(media.Url, media.AltText))
                    .FirstOrDefault()
            })
            .ToListAsync();

        var productCards = productData.Select(product =>
        {
            var productUrl = "/categories";
            if (CatalogRoutingHelper.TryGetPrimaryCategorySlug(product.Slug, out var categorySlug))
            {
                var category = categoryNodes.FirstOrDefault(item => item.Slug.Equals(categorySlug, StringComparison.OrdinalIgnoreCase));
                if (category is not null)
                {
                    var chain = CatalogRoutingHelper.BuildCategoryChain(categoryNodes, category);
                    productUrl = CatalogRoutingHelper.BuildProductPath(chain, product.Slug);
                }
            }

            return new ProductCardViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Url = productUrl,
                ShortDescription = SeoContentHelper.BuildShortDescription(SeoContentHelper.ExtractDescription(product.Description)),
                ImageUrl = product.PrimaryMedia?.Url,
                ImageAltText = product.PrimaryMedia?.AltText ?? product.Name,
                PriceDisplay = LocalizationHelper.FormatPrice(product.Price, product.PriceType),
                CanPurchaseOnline = product.PriceType == PriceType.Fixed && product.Price > 0
            };
        }).ToList();

        var postData = await DbContext.BlogPosts
            .AsNoTracking()
            .Where(post => post.PublishedAt != null)
            .OrderByDescending(post => post.PublishedAt)
            .Take(3)
            .Select(post => new
            {
                post.Title,
                post.Slug,
                post.Excerpt,
                post.FeaturedMediaId,
                post.PublishedAt
            })
            .ToListAsync();

        var mediaIds = new HashSet<int>();

        foreach (var post in postData)
        {
            if (post.FeaturedMediaId.HasValue)
            {
                mediaIds.Add(post.FeaturedMediaId.Value);
            }
        }

        foreach (var category in topCategoryData)
        {
            if (category.MediaId.HasValue)
            {
                mediaIds.Add(category.MediaId.Value);
            }
        }

        if (settings?.HomeHeroMediaId is not null)
        {
            mediaIds.Add(settings.HomeHeroMediaId.Value);
        }

        if (settings?.HomeSecondaryMediaId is not null)
        {
            mediaIds.Add(settings.HomeSecondaryMediaId.Value);
        }

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

        var fallbackCategoryMedia = settings?.DefaultCategoryMediaId is not null
            && mediaMap.TryGetValue(settings.DefaultCategoryMediaId.Value, out var fallbackMedia)
            ? fallbackMedia
            : null;

        var topCategories = topCategoryData.Select(category =>
        {
            var media = category.MediaId.HasValue && mediaMap.TryGetValue(category.MediaId.Value, out var categoryMedia)
                ? categoryMedia
                : fallbackCategoryMedia;

            return new CategoryCardViewModel
            {
                Name = category.Name,
                Url = "/" + category.Slug,
                ImageUrl = media?.Url,
                ImageAltText = media?.AltText ?? category.Name,
                Summary = BuildCategorySummary(category.Name)
            };
        }).ToList();

        var latestPosts = postData.Select(post => new BlogCardViewModel
        {
            Title = post.Title,
            Url = Url.Action("Details", "Blog", new { slug = post.Slug }) ?? $"/blog/{post.Slug}",
            Excerpt = post.Excerpt,
            ImageUrl = post.FeaturedMediaId.HasValue && mediaMap.TryGetValue(post.FeaturedMediaId.Value, out var media) ? media.Url : null,
            PublishedAt = LocalizationHelper.FormatDate(post.PublishedAt)
        }).ToList();

        var projectData = await DbContext.Projects
            .AsNoTracking()
            .Where(project => project.IsPublished)
            .OrderByDescending(project => project.CreatedAt)
            .Take(3)
            .Select(project => new
            {
                project.Title,
                project.Description,
                project.Slug,
                project.FeaturedMediaId,
                PrimaryMedia = project.Gallery
                    .OrderBy(media => media.Id)
                    .Select(media => new MediaSnapshot(media.Url, media.AltText))
                    .FirstOrDefault()
            })
            .ToListAsync();

        var projectFeaturedIds = projectData
            .Where(project => project.FeaturedMediaId.HasValue)
            .Select(project => project.FeaturedMediaId!.Value)
            .ToHashSet();

        var projectMediaMap = projectFeaturedIds.Count == 0
            ? new Dictionary<int, MediaSnapshot>()
            : await DbContext.Media
                .AsNoTracking()
                .Where(media => projectFeaturedIds.Contains(media.Id))
                .ToDictionaryAsync(media => media.Id, media => new MediaSnapshot(media.Url, media.AltText));

        var projects = projectData.Select(project => new ProjectCardViewModel
        {
            Title = project.Title,
            Summary = project.Description ?? string.Empty,            
            Url = Url.Action("Details", "Projects", new { slug = project.Slug }) ?? $"/projects/{project.Slug}",
            ImageUrl = project.FeaturedMediaId.HasValue && projectMediaMap.TryGetValue(project.FeaturedMediaId.Value, out var featuredMedia)
                ? featuredMedia.Url
                : project.PrimaryMedia?.Url,
            ImageAltText = project.FeaturedMediaId.HasValue && projectMediaMap.TryGetValue(project.FeaturedMediaId.Value, out var featuredMediaAlt)
                ? featuredMediaAlt.AltText ?? project.Title
                : project.PrimaryMedia?.AltText ?? project.Title
        }).ToList();

        var model = new HomeViewModel
        {
            HeroTitle = "قطعات تخصصی پژو ۲۰۰۸",
            HeroSubtitle = "خرید مطمئن قطعات اصلی و باکیفیت با بررسی فنی، ضمانت اصالت و ارسال سراسری.",
            HeroImageUrl = settings?.HomeHeroMediaId is not null && mediaMap.TryGetValue(settings.HomeHeroMediaId.Value, out var heroMedia)
                ? heroMedia.Url
                : null,
            HeroImageAltText = settings?.HomeHeroMediaId is not null && mediaMap.TryGetValue(settings.HomeHeroMediaId.Value, out var heroAltMedia)
                ? heroAltMedia.AltText
                : null,
            SecondaryImageUrl = settings?.HomeSecondaryMediaId is not null && mediaMap.TryGetValue(settings.HomeSecondaryMediaId.Value, out var secondaryMedia)
                ? secondaryMedia.Url
                : null,
            SecondaryImageAltText = settings?.HomeSecondaryMediaId is not null && mediaMap.TryGetValue(settings.HomeSecondaryMediaId.Value, out var secondaryAltMedia)
                ? secondaryAltMedia.AltText
                : null,
            TopCategories = topCategories,
            FeaturedProducts = productCards,
            LatestPosts = latestPosts,
            FeaturedProjects = projects
        };

        var canonicalUrl = Url.Action("Index", "Home", new { }, Request.Scheme);
        model.MetaTitle = "mr2008.ir | فروش تخصصی قطعات پژو ۲۰۰۸";
        model.MetaDescription = "مرجع تخصصی خرید قطعات پژو ۲۰۰۸ با تضمین اصالت، مشاوره فنی و ارسال مطمئن به سراسر ایران.";
        model.CanonicalUrl = canonicalUrl ?? string.Empty;
        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.WebPage,
            Title = "mr2008.ir | فروش تخصصی قطعات پژو ۲۰۰۸",
            Description = "مرجع تخصصی خرید قطعات پژو ۲۰۰۸ با تضمین اصالت، مشاوره فنی و ارسال مطمئن به سراسر ایران.",
            CanonicalUrl = canonicalUrl ?? string.Empty
        });

        return View(model);
    }

    private static string BuildCategorySummary(string categoryName)
    {
        return $"قطعات منتخب {categoryName} با امکان بررسی فنی پیش از خرید.";
    }

    private sealed record MediaSnapshot(string Url, string? AltText);
}
