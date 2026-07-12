using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class BlogPostIndexViewModel
{
    public IReadOnlyList<BlogPostListItemViewModel> Posts { get; set; } = Array.Empty<BlogPostListItemViewModel>();
}

public class BlogPostListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
}

public class BlogPostFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "عنوان مقاله الزامی است.")]
    [Display(Name = "عنوان")]
    [StringLength(200, ErrorMessage = "عنوان حداکثر ۲۰۰ کاراکتر است.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "اسلاگ")]
    [StringLength(200, ErrorMessage = "اسلاگ حداکثر ۲۰۰ کاراکتر است.")]
    public string? Slug { get; set; }

    [Required(ErrorMessage = "خلاصه مقاله الزامی است.")]
    [Display(Name = "خلاصه")]
    [StringLength(600, ErrorMessage = "خلاصه حداکثر ۶۰۰ کاراکتر است.")]
    public string Excerpt { get; set; } = string.Empty;

    [Required(ErrorMessage = "محتوا الزامی است.")]
    [Display(Name = "محتوا")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "تصویر شاخص")]
    public int? FeaturedMediaId { get; set; }

    public string? FeaturedMediaUrl { get; set; }

    [Display(Name = "عنوان متا")]
    [StringLength(200, ErrorMessage = "عنوان متا حداکثر ۲۰۰ کاراکتر است.")]
    public string? MetaTitle { get; set; }

    [Display(Name = "توضیح متا")]
    [StringLength(300, ErrorMessage = "توضیح متا حداکثر ۳۰۰ کاراکتر است.")]
    public string? MetaDescription { get; set; }

}
