using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Web.Services;
using Atelier.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Controllers;

[Route("pages")]
public class PagesController : PublicControllerBase
{
    public PagesController(AtelierDbContext dbContext) : base(dbContext)
    {
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var page = await GetPageSnapshotAsync(slug);
        if (page is null)
        {
            return NotFound();
        }

        var breadcrumbs = BuildBreadcrumbs(page.Title);
        var canonicalUrl = Url.Action("Details", "Pages", new { slug = page.Slug }, Request.Scheme);
        var meta = BuildMeta(page, canonicalUrl);
        var featuredImageUrl = await GetMediaUrlAsync(page.FeaturedMediaId);

        if (IsContactPage(slug))
        {
            var m = BuildContactPageModel(page, breadcrumbs, meta.CanonicalUrl, featuredImageUrl, new ContactFormInputModel(), meta.Title, meta.Description);
            return View("Contact", m);
        }

        var model = new PageViewModel
        {
            Title = page.Title,
            Content = page.Content,
            Breadcrumbs = breadcrumbs,
            MetaTitle = meta.Title,
            MetaDescription = meta.Description,
            CanonicalUrl = meta.CanonicalUrl
        };

        SetSeoMetadata(meta.Title, meta.Description, canonicalUrl, featuredImageUrl);
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.WebPage,
            Title = meta.Title,
            Description = meta.Description,
            CanonicalUrl = canonicalUrl ?? string.Empty
        });

        return View(model);
    }

    [HttpPost("contact-us")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitContact(ContactFormInputModel input)
    {
        var page = await GetPageSnapshotAsync("contact-us");
        if (page is null)
        {
            return NotFound();
        }

        var breadcrumbs = BuildBreadcrumbs(page.Title);
        var canonicalUrl = Url.Action("Details", "Pages", new { slug = page.Slug }, Request.Scheme);
        var meta = BuildMeta(page, canonicalUrl);
        var featuredImageUrl = await GetMediaUrlAsync(page.FeaturedMediaId);

        if (!ModelState.IsValid)
        {
            var model = BuildContactPageModel(page, breadcrumbs, meta.CanonicalUrl, featuredImageUrl, input, meta.Title, meta.Description);
            return View("Contact", model);
        }

        var message = input.Message;
        if (!string.IsNullOrWhiteSpace(input.Phone))
        {
            message = $"{message}\nشماره تماس: {input.Phone}";
        }

        var lead = new Lead(input.Name, input.Email, message);
        DbContext.Leads.Add(lead);
        await DbContext.SaveChangesAsync();

        ModelState.Clear();

        var successModel = BuildContactPageModel(page, breadcrumbs, meta.CanonicalUrl, featuredImageUrl, new ContactFormInputModel(), meta.Title, meta.Description);
        successModel.Submitted = true;
        return View("Contact", successModel);
    }

    private async Task<PageSnapshot?> GetPageSnapshotAsync(string slug)
    {
        return await DbContext.Pages
            .AsNoTracking()
            .Where(item => item.Slug == slug && item.Status == PageStatus.Published)
            .Select(item => new PageSnapshot
            {
                Title = item.Title,
                Slug = item.Slug,
                Content = item.Content,
                FeaturedMediaId = item.FeaturedMediaId,
                MetaTitle = item.MetaTitle,
                MetaDescription = item.MetaDescription
            })
            .FirstOrDefaultAsync();
    }

    private static bool IsContactPage(string slug) =>
        slug.Equals("contact-us", StringComparison.OrdinalIgnoreCase);

    private List<BreadcrumbItemViewModel> BuildBreadcrumbs(string title)
    {
        var breadcrumbs = new List<BreadcrumbItemViewModel>
        {
            new() { Title = "خانه", Url = Url.Action("Index", "Home") },
            new() { Title = title }
        };

        return breadcrumbs;
    }

    private (string Title, string Description, string CanonicalUrl) BuildMeta(PageSnapshot page, string? canonicalUrl)
    {
        var metaTitle = string.IsNullOrWhiteSpace(page.MetaTitle) ? page.Title : page.MetaTitle;
        var metaDescription = string.IsNullOrWhiteSpace(page.MetaDescription)
            ? SeoContentHelper.BuildMetaDescription(page.Content)
            : page.MetaDescription;

        var canonical = canonicalUrl ?? string.Empty;
        return (metaTitle, metaDescription ?? page.Title, canonical);
    }

    private ContactPageViewModel BuildContactPageModel(
        PageSnapshot page,
        List<BreadcrumbItemViewModel> breadcrumbs,
        string canonicalUrl,
        string? featuredImageUrl,
        ContactFormInputModel inputModel,
        string metaTitle,
        string metaDescription)
    {
        var form = new ContactFormViewModel
        {
            Title = "فرم تماس با ما",
            Description = "برای دریافت مشاوره، استعلام موجودی و ثبت سفارش اطلاعات خود را وارد کنید.",
            ActionUrl = Url.Action("SubmitContact", "Pages") ?? "/pages/contact-us",
            Fields = new List<FormFieldViewModel>
            {
                new() { Name = nameof(ContactFormInputModel.Name), Label = "نام و نام خانوادگی", Type = "text", Placeholder = "مثال: علی رضایی", IsRequired = true },
                new() { Name = nameof(ContactFormInputModel.Email), Label = "ایمیل", Type = "email", Placeholder = "example@email.com", IsRequired = true },
                new() { Name = nameof(ContactFormInputModel.Phone), Label = "شماره تماس", Type = "tel", Placeholder = "09xx xxx xxxx", IsRequired = false },
                new() { Name = nameof(ContactFormInputModel.Message), Label = "پیام شما", Type = "textarea", Placeholder = "موضوع یا توضیحات مورد نیاز را بنویسید", IsRequired = true }
            }
        };

        var model = new ContactPageViewModel
        {
            Title = page.Title,
            Content = page.Content,
            Breadcrumbs = breadcrumbs,
            MetaTitle = metaTitle,
            MetaDescription = metaDescription,
            CanonicalUrl = canonicalUrl,
            Form = form,
            FormInput = inputModel
        };

        SetSeoMetadata(model.MetaTitle, model.MetaDescription, canonicalUrl, featuredImageUrl);
        SetBreadcrumbSchema(breadcrumbs);
        ViewData["PageSchema"] = SeoHelper.BuildPageSchema(new SeoPageSchemaData
        {
            Type = SeoPageSchemaType.WebPage,
            Title = model.MetaTitle,
            Description = model.MetaDescription,
            CanonicalUrl = canonicalUrl
        });

        return model;
    }

    private sealed class PageSnapshot
    {
        public string Title { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public string? Content { get; init; }
        public int? FeaturedMediaId { get; init; }
        public string? MetaTitle { get; init; }
        public string? MetaDescription { get; init; }
    }
}
