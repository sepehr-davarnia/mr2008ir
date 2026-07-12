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

    public SitemapController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        var blogPosts = await _dbContext.BlogPosts
            .AsNoTracking()
            .Where(post => post.PublishedAt != null)
            .Select(post => new { post.Slug, post.UpdatedAt, post.PublishedAt })
            .ToListAsync();

        var pages = await _dbContext.Pages
            .AsNoTracking()
            .Where(page => page.Status == PageStatus.Published)
            .Select(page => new { page.Slug, page.UpdatedAt, page.CreatedAt })
            .ToListAsync();

        var projects = await _dbContext.Projects
            .AsNoTracking()
            .Where(project => project.IsPublished)
            .Select(project => new { project.Slug, project.UpdatedAt, project.CreatedAt })
            .ToListAsync();

        var basePath = Request.PathBase.Add("/").ToString().TrimEnd('/');
        var baseUrl = SeoHelper.BuildAbsoluteUrl(Request, basePath);

        var urls = new List<SitemapUrl>();
        urls.AddRange(blogPosts.Select(post => new SitemapUrl
        {
            Location = Url.Action("Details", "Blog", new { slug = post.Slug }, Request.Scheme) ?? string.Empty,
            LastModified = post.UpdatedAt ?? post.PublishedAt?.UtcDateTime,
            ChangeFrequency = "weekly",
            Priority = "0.6"
        }));

        urls.AddRange(pages.Select(page => new SitemapUrl
        {
            Location = string.Concat(baseUrl, "/pages/", page.Slug),
            LastModified = page.UpdatedAt ?? page.CreatedAt,
            ChangeFrequency = "monthly",
            Priority = "0.5"
        }));

        urls.AddRange(projects.Select(project => new SitemapUrl
        {
            Location = string.Concat(baseUrl, "/projects/", project.Slug),
            LastModified = project.UpdatedAt ?? project.CreatedAt,
            ChangeFrequency = "monthly",
            Priority = "0.5"
        }));

        var sitemap = new XDocument(
            new XElement("urlset",
                new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                urls.Where(url => !string.IsNullOrWhiteSpace(url.Location)).Select(url =>
                    new XElement("url",
                        new XElement("loc", url.Location),
                        url.LastModified.HasValue ? new XElement("lastmod", url.LastModified.Value.ToString("yyyy-MM-dd")) : null,
                        new XElement("changefreq", url.ChangeFrequency),
                        new XElement("priority", url.Priority))
                )));

        return Content(sitemap.ToString(SaveOptions.DisableFormatting), "application/xml", Encoding.UTF8);
    }

    private sealed class SitemapUrl
    {
        public string Location { get; init; } = string.Empty;
        public DateTime? LastModified { get; init; }
        public string ChangeFrequency { get; init; } = "monthly";
        public string Priority { get; init; } = "0.5";
    }
}
