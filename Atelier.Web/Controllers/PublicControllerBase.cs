using Atelier.Infrastructure.Data;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

public abstract class PublicControllerBase : Controller
{
    protected readonly AtelierDbContext DbContext;

    protected PublicControllerBase(AtelierDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await EnsureLayoutAsync();
        await next();
    }

    protected void SetSeoMetadata(string title, string description, string? canonicalUrl = null, string? imageUrl = null, string ogType = "website")
    {
        ViewData["Title"] = title;
        ViewData["MetaTitle"] = title;
        ViewData["MetaDescription"] = description;
        ViewData["CanonicalUrl"] = canonicalUrl;
        ViewData["OpenGraphTitle"] = title;
        ViewData["OpenGraphDescription"] = description;
        ViewData["OpenGraphImage"] = imageUrl;
        ViewData["OpenGraphType"] = ogType;
    }

    protected void SetBreadcrumbSchema(IEnumerable<BreadcrumbItemViewModel> breadcrumbs)
    {
        var canonicalUrl = SeoHelper.BuildCanonicalUrl(Request, ViewData["CanonicalUrl"]?.ToString());
        var items = breadcrumbs
            .Select((item, index) => new SeoBreadcrumbItem
            {
                Name = item.Title,
                Url = string.IsNullOrWhiteSpace(item.Url) ? canonicalUrl : item.Url,
                Position = index + 1
            })
            .ToList();

        ViewData["BreadcrumbSchema"] = SeoHelper.BuildBreadcrumbSchema(items);
    }

    protected async Task<string?> GetMediaUrlAsync(int? mediaId)
    {
        if (!mediaId.HasValue)
        {
            return null;
        }

        return await DbContext.Media
            .AsNoTracking()
            .Where(media => media.Id == mediaId.Value)
            .Select(media => media.Url)
            .FirstOrDefaultAsync();
    }

    private async Task EnsureLayoutAsync()
    {
        if (ViewData.ContainsKey("Layout"))
        {
            return;
        }

        var settings = await DbContext.SiteSettings
            .AsNoTracking()
            .FirstOrDefaultAsync();

        var siteName = string.IsNullOrWhiteSpace(settings?.SiteName) ? "mr2008.ir" : settings!.SiteName!;
        var logoUrl = await GetMediaUrlAsync(settings?.LogoMediaId);
        var faviconUrl = await GetMediaUrlAsync(settings?.FaviconMediaId);

        var primaryLinks = new List<NavigationLinkViewModel>
        {
            new() { Title = "خانه", Url = Url.Action("Index", "Home") ?? "/" },
            new() { Title = "فروشگاه قطعات", Url = Url.Action("Index", "Catalog") ?? "/categories" },
            new() { Title = "راهنمای خرید", Url = Url.Action("Index", "Blog") ?? "/blog" },
            new() { Title = "تماس با ما", Url = "/pages/contact-us" }
        };

        var categoryLinks = await DbContext.Categories
            .AsNoTracking()
            .Where(category => EF.Property<int?>(category, "ParentId") == null)
            .OrderBy(category => category.Name)
            .Select(category => new NavigationLinkViewModel
            {
                Title = category.Name,
                Url = "/" + category.Slug
            })
            .ToListAsync();

        var socialLinks = new List<NavigationLinkViewModel>();
        if (!string.IsNullOrWhiteSpace(settings?.Instagram))
        {
            socialLinks.Add(new NavigationLinkViewModel { Title = "اینستاگرام", Url = settings.Instagram });
        }
        if (!string.IsNullOrWhiteSpace(settings?.Telegram))
        {
            socialLinks.Add(new NavigationLinkViewModel { Title = "تلگرام", Url = settings.Telegram });
        }
        if (!string.IsNullOrWhiteSpace(settings?.WhatsApp))
        {
            socialLinks.Add(new NavigationLinkViewModel { Title = "واتساپ", Url = settings.WhatsApp });
        }

        var layout = new LayoutViewModel
        {
            SiteName = siteName,
            Tagline = "مرجع تخصصی قطعات پژو ۲۰۰۸",
            LogoUrl = logoUrl,
            FaviconUrl = faviconUrl,
            Address = settings?.Address,
            Phone = settings?.Phone ?? settings?.Mobile,
            WhatsApp = settings?.WhatsApp,
            Email = settings?.Email,
            PrimaryLinks = primaryLinks,
            CategoryLinks = categoryLinks,
            SocialLinks = socialLinks
        };

        ViewData["Layout"] = layout;

        var basePath = Request.PathBase.Add("/").ToString().TrimEnd('/');
        var siteUrl = SeoHelper.BuildAbsoluteUrl(Request, basePath);
        ViewData["OrganizationSchema"] = SeoHelper.BuildOrganizationSchema(new SeoOrganizationSchemaData
        {
            Name = siteName,
            Url = siteUrl,
            LogoUrl = logoUrl,
            SameAs = socialLinks.Select(link => link.Url).ToList()
        });
        ViewData["WebsiteSchema"] = SeoHelper.BuildWebsiteSchema(new SeoWebsiteSchemaData
        {
            Name = siteName,
            Url = siteUrl
        });
    }
}
