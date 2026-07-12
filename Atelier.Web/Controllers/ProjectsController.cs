using Atelier.Infrastructure.Data;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

[Route("projects")]
public class ProjectsController : PublicControllerBase
{
    public ProjectsController(AtelierDbContext dbContext) : base(dbContext)
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var projects = await DbContext.Projects
            .AsNoTracking()
            .Where(project => project.IsPublished)
            .OrderByDescending(project => project.CreatedAt)
            .Select(project => new
            {
                project.Title,
                project.Description,
                project.Slug,
                project.FeaturedMediaId,
                PrimaryMedia = project.Gallery
                    .OrderBy(media => media.Id)
                    .Select(media => new MediaItemViewModel
                    {
                        Url = media.Url,
                        AltText = media.AltText
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        var featuredMediaIds = projects
            .Where(project => project.FeaturedMediaId.HasValue)
            .Select(project => project.FeaturedMediaId!.Value)
            .ToHashSet();

        var featuredMediaMap = featuredMediaIds.Count == 0
            ? new Dictionary<int, MediaItemViewModel>()
            : await DbContext.Media
                .AsNoTracking()
                .Where(media => featuredMediaIds.Contains(media.Id))
                .ToDictionaryAsync(
                    media => media.Id,
                    media => new MediaItemViewModel { Url = media.Url, AltText = media.AltText });

        var projectItems = projects.Select(project => new ProjectListItemViewModel
        {
            Title = project.Title,
            Summary = project.Description,
            Url = Url.Action("Details", "Projects", new { slug = project.Slug }) ?? $"/projects/{project.Slug}",
            ImageUrl = project.FeaturedMediaId.HasValue && featuredMediaMap.TryGetValue(project.FeaturedMediaId.Value, out var featuredMedia)
                ? featuredMedia.Url
                : project.PrimaryMedia?.Url,
            ImageAltText = project.FeaturedMediaId.HasValue && featuredMediaMap.TryGetValue(project.FeaturedMediaId.Value, out var featuredMediaAlt)
                ? featuredMediaAlt.AltText ?? project.Title
                : project.PrimaryMedia?.AltText ?? project.Title
        }).ToList();

        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "خانه", Url = Url.Action("Index", "Home") },
            new() { Title = "پروژه ها" }
        };

        var model = new ProjectListViewModel
        {
            Projects = projectItems,
            Breadcrumbs = breadcrumbs
        };

        var canonicalUrl = Url.Action("Index", "Projects", new { }, Request.Scheme);
        model.MetaTitle = "خدمات و تجربه‌های فنی پژو ۲۰۰۸ | mr2008.ir";
        model.MetaDescription = "نمونه خدمات، بررسی‌های فنی و تجربه‌های تخصصی مرتبط با قطعات پژو ۲۰۰۸.";
        model.CanonicalUrl = canonicalUrl ?? string.Empty;
        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl);
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.Project,
            Title = "خدمات و تجربه‌های فنی پژو ۲۰۰۸ | mr2008.ir",
            Description = "نمونه خدمات، بررسی‌های فنی و تجربه‌های تخصصی مرتبط با قطعات پژو ۲۰۰۸.",
            CanonicalUrl = canonicalUrl ?? string.Empty
        });

        return View(model);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var project = await DbContext.Projects
            .AsNoTracking()
            .Where(item => item.Slug == slug && item.IsPublished)
            .Select(item => new
            {
                item.Title,
                item.Description,
                item.FeaturedMediaId,
                item.Slug,
                Gallery = item.Gallery
                    .OrderBy(media => media.Id)
                    .Select(media => new MediaItemViewModel
                    {
                        Url = media.Url,
                        AltText = media.AltText
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (project is null)
        {
            return NotFound();
        }

        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "خانه", Url = Url.Action("Index", "Home") },
            new() { Title = "پروژه ها", Url = Url.Action("Index", "Projects") },
            new() { Title = project.Title }
        };

        var heroImageUrl = await GetMediaUrlAsync(project.FeaturedMediaId) ?? project.Gallery.FirstOrDefault()?.Url;
        var model = new ProjectDetailViewModel
        {
            Title = project.Title,
            Summary = SeoContentHelper.BuildShortDescription(project.Description),
            Description = project.Description,
            HeroImageUrl = heroImageUrl,
            HeroImageAltText = project.Title,
            Gallery = project.Gallery,
            Breadcrumbs = breadcrumbs
        };

        var canonicalUrl = Url.Action("Details", "Projects", new { slug }, Request.Scheme);
        model.MetaTitle = project.Title;
        model.MetaDescription = SeoContentHelper.BuildMetaDescription(project.Description) ?? project.Title;
        model.CanonicalUrl = canonicalUrl ?? string.Empty;

        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl, heroImageUrl, "article");
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.Project,
            Title = model.MetaTitle,
            Description = model.MetaDescription,
            CanonicalUrl = canonicalUrl ?? string.Empty
        });

        return View(model);
    }
}
