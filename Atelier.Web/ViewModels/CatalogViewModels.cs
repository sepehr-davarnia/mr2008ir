using System.Collections.Generic;

namespace Atelier.Web.ViewModels;

public class HomeViewModel
{
    public string HeroTitle { get; set; } = string.Empty;
    public string HeroSubtitle { get; set; } = string.Empty;
    public string? HeroImageUrl { get; set; }
    public string? HeroImageAltText { get; set; }
    public string? SecondaryImageUrl { get; set; }
    public string? SecondaryImageAltText { get; set; }
    public IReadOnlyList<CategoryCardViewModel> TopCategories { get; set; } = new List<CategoryCardViewModel>();
    public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; set; } = new List<ProductCardViewModel>();
    public IReadOnlyList<BlogCardViewModel> LatestPosts { get; set; } = new List<BlogCardViewModel>();
    public IReadOnlyList<ProjectCardViewModel> FeaturedProjects { get; set; } = new List<ProjectCardViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

public class CategoryViewModel
{
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = new List<BreadcrumbItemViewModel>();
    public IReadOnlyList<CategoryTreeItemViewModel> Categories { get; set; } = new List<CategoryTreeItemViewModel>();
    public string? Query { get; set; }
    public IReadOnlyList<ProductCardViewModel> SearchResults { get; set; } = new List<ProductCardViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

public class ProductListViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? HeroMediaUrl { get; set; }
    public string? HeroMediaAltText { get; set; }
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = new List<BreadcrumbItemViewModel>();
    public IReadOnlyList<CategoryCardViewModel> Subcategories { get; set; } = new List<CategoryCardViewModel>();
    public IReadOnlyList<ProductCardViewModel> Products { get; set; } = new List<ProductCardViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

public class ProductDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string PriceDisplay { get; set; } = string.Empty;
    public bool CanPurchaseOnline { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? HeroImageAltText { get; set; }
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = new List<BreadcrumbItemViewModel>();
    public IReadOnlyList<ProductSpecificationViewModel> Specifications { get; set; } = new List<ProductSpecificationViewModel>();
    public IReadOnlyList<MediaItemViewModel> Gallery { get; set; } = new List<MediaItemViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

public class CategoryTreeItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
    public string Summary { get; set; } = string.Empty;
    public IReadOnlyList<CategoryTreeItemViewModel> Children { get; set; } = new List<CategoryTreeItemViewModel>();
}

public class CategoryCardViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
    public string PriceDisplay { get; set; } = string.Empty;
    public bool CanPurchaseOnline { get; set; }
}

public class ProductSpecificationViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class MediaItemViewModel
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
}

public class BlogCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string PublishedAt { get; set; } = string.Empty;
}

public class ProjectCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
    public string? Url { get; set; }
    public string? Slug { get; set; }
}
