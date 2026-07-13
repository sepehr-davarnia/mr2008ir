using Atelier.Infrastructure.Data;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

[Route("blog")]
public class BlogController : PublicControllerBase
{
    public BlogController(AtelierDbContext dbContext) : base(dbContext)
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var posts = await DbContext.BlogPosts
            .AsNoTracking()
            .Where(post => post.PublishedAt != null)
            .OrderByDescending(post => post.PublishedAt)
            .Select(post => new
            {
                post.Title,
                post.Slug,
                post.Excerpt,
                post.FeaturedMediaId,
                post.PublishedAt
            })
            .ToListAsync();

        var mediaIds = posts
            .Where(post => post.FeaturedMediaId.HasValue)
            .Select(post => post.FeaturedMediaId!.Value)
            .Distinct()
            .ToList();

        var mediaMap = mediaIds.Count == 0
            ? new Dictionary<int, string?>()
            : await DbContext.Media
                .AsNoTracking()
                .Where(media => mediaIds.Contains(media.Id))
                .ToDictionaryAsync(media => media.Id, media => media.Url);

        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "خانه", Url = Url.Action("Index", "Home") },
            new() { Title = "بلاگ" }
        };

        var model = new BlogListViewModel
        {
            Posts = posts.Select(post => new BlogListItemViewModel
            {
                Title = post.Title,
                Slug = post.Slug,
                Excerpt = post.Excerpt,
                FeaturedImageUrl = post.FeaturedMediaId.HasValue && mediaMap.TryGetValue(post.FeaturedMediaId.Value, out var url)
                    ? url
                    : null,
                PublishedAt = post.PublishedAt!.Value
            }).ToList(),
            Breadcrumbs = breadcrumbs
        };

        var canonicalUrl = Url.Action("Index", "Blog", new { }, Request.Scheme);
        model.MetaTitle = "راهنمای خرید و نگهداری پژو ۲۰۰۸ | mr2008.ir";
        model.MetaDescription = "راهنمای تخصصی انتخاب قطعات، نگهداری و خرید مطمئن لوازم پژو ۲۰۰۸.";
        model.CanonicalUrl = canonicalUrl ?? string.Empty;
        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl);
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.WebPage,
            Title = "راهنمای خرید و نگهداری پژو ۲۰۰۸ | mr2008.ir",
            Description = "راهنمای تخصصی انتخاب قطعات، نگهداری و خرید مطمئن لوازم پژو ۲۰۰۸.",
            CanonicalUrl = canonicalUrl ?? string.Empty
        });

        return View(model);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var post = await DbContext.BlogPosts
            .AsNoTracking()
            .Where(item => item.Slug == slug && item.PublishedAt != null)
            .Select(item => new
            {
                item.Title,
                item.Slug,
                item.Excerpt,
                item.Content,
                item.FeaturedMediaId,
                item.MetaTitle,
                item.MetaDescription,
                item.PublishedAt
            })
            .FirstOrDefaultAsync();

        if (post is null)
        {
            return NotFound();
        }

        string? featuredUrl = null;
        if (post.FeaturedMediaId.HasValue)
        {
            featuredUrl = await DbContext.Media
                .AsNoTracking()
                .Where(media => media.Id == post.FeaturedMediaId)
                .Select(media => media.Url)
                .FirstOrDefaultAsync();
        }

        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "خانه", Url = Url.Action("Index", "Home") },
            new() { Title = "بلاگ", Url = Url.Action("Index", "Blog") },
            new() { Title = post.Title }
        };

        var model = new BlogDetailsViewModel
        {
            Title = post.Title,
            Excerpt = post.Excerpt,
            Content = post.Content,
            FeaturedImageUrl = featuredUrl,
            PublishedAt = post.PublishedAt!.Value,
            Breadcrumbs = breadcrumbs
        };

        var canonicalUrl = Url.Action("Details", "Blog", new { slug = post.Slug }, Request.Scheme);
        var metaTitle = string.IsNullOrWhiteSpace(post.MetaTitle) ? post.Title : post.MetaTitle;
        var metaDescription = string.IsNullOrWhiteSpace(post.MetaDescription) ? post.Excerpt : post.MetaDescription;
        model.MetaTitle = metaTitle;
        model.MetaDescription = metaDescription;
        model.CanonicalUrl = canonicalUrl ?? string.Empty;
        SetSeoMetadata(metaTitle, metaDescription, canonicalUrl, featuredUrl, "article");
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.BlogPost,
            Title = post.Title,
            Description = metaDescription,
            CanonicalUrl = canonicalUrl ?? string.Empty,
            PublishedAt = post.PublishedAt,
            ImageUrl = featuredUrl
        });

        return View(model);
    }
}
