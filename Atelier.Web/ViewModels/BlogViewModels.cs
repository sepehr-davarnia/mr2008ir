using System;
using System.Collections.Generic;

namespace Atelier.Web.ViewModels;

public class BlogListViewModel
{
    public IReadOnlyList<BlogListItemViewModel> Posts { get; set; } = Array.Empty<BlogListItemViewModel>();
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = Array.Empty<BreadcrumbItemViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

public class BlogListItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
}

public class BlogDetailsViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = Array.Empty<BreadcrumbItemViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}
