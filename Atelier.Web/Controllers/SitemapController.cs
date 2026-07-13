using System.Text;
using System.Xml.Linq;
using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

public class SitemapController : Controller
{
    private readonly AtelierDbContext _dbContext;

    public SitemapController(AtelierDbContext dbContext) => _dbContext = dbContext;

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Index()
    {
        var categories = await _dbContext.Categories.AsNoTracking()
            .Select(category => new CatalogCategoryNode
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ParentId = EF.Property<int?>(category, "ParentId"),
                MediaId = category.MediaId,
                UpdatedAt = category.UpdatedAt ?? category.CreatedAt
            }).ToListAsync();

        var products = await _dbContext.Products.AsNoTracking()
            .Where(product => product.Status == ProductStatus.Published)
            .Select(product => new
            {
                product.Slug, product.UpdatedAt, product.CreatedAt,
                PrimaryCategoryId = product.Categories.OrderBy(category => category.Id).Select(category => (int?)category.Id).FirstOrDefault()
            })
            .ToListAsync();
        var posts = await _dbContext.BlogPosts.AsNoTracking()
            .Where(post => post.PublishedAt != null)
            .Select(post => new { post.Slug, post.UpdatedAt, post.PublishedAt }).ToListAsync();
        var pages = await _dbContext.Pages.AsNoTracking()
            .Where(page => page.Status == PageStatus.Published)
            .Select(page => new { page.Slug, page.UpdatedAt, page.CreatedAt }).ToListAsync();

        var baseUrl = SeoHelper.BuildAbsoluteUrl(Request, Request.PathBase.Add("/").ToString().TrimEnd('/'));
        var urls = new List<SitemapUrl>
        {
            new(baseUrl + "/", null, "daily", "1.0"),
            new(baseUrl + "/categories", null, "daily", "0.9"),
            new(baseUrl + "/blog", null, "weekly", "0.7")
        };

        urls.AddRange(categories.Select(category => new SitemapUrl(
            baseUrl + CatalogRoutingHelper.BuildCategoryPath(categories, category),
            category.UpdatedAt, "weekly", "0.8")));

        foreach (var product in products)
        {
            var category = categories.FirstOrDefault(item => item.Id == product.PrimaryCategoryId);
            if (category is null) continue;
            var chain = CatalogRoutingHelper.BuildCategoryChain(categories, category);
            urls.Add(new SitemapUrl(baseUrl + CatalogRoutingHelper.BuildProductPath(chain, product.Slug),
                product.UpdatedAt ?? product.CreatedAt, "weekly", "0.8"));
        }

        urls.AddRange(posts.Select(post => new SitemapUrl(baseUrl + "/blog/" + post.Slug,
            post.UpdatedAt ?? post.PublishedAt?.UtcDateTime, "weekly", "0.6")));
        urls.AddRange(pages.Select(page => new SitemapUrl(baseUrl + "/pages/" + page.Slug,
            page.UpdatedAt ?? page.CreatedAt, "monthly", "0.5")));

        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemap = new XDocument(new XElement(ns + "urlset",
            urls.Where(item => !string.IsNullOrWhiteSpace(item.Location)).DistinctBy(item => item.Location).Select(item =>
                new XElement(ns + "url",
                    new XElement(ns + "loc", item.Location),
                    item.LastModified.HasValue ? new XElement(ns + "lastmod", item.LastModified.Value.ToString("yyyy-MM-dd")) : null,
                    new XElement(ns + "changefreq", item.ChangeFrequency),
                    new XElement(ns + "priority", item.Priority)))));
        return Content(sitemap.ToString(SaveOptions.DisableFormatting), "application/xml", Encoding.UTF8);
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 3600)]
    public IActionResult Robots()
    {
        var baseUrl = SeoHelper.BuildAbsoluteUrl(Request, Request.PathBase.Add("/").ToString().TrimEnd('/'));
        return Content($"User-agent: *\nAllow: /\nDisallow: /Admin/\nSitemap: {baseUrl}/sitemap.xml\n", "text/plain", Encoding.UTF8);
    }

    private sealed record SitemapUrl(string Location, DateTime? LastModified, string ChangeFrequency, string Priority);
}
