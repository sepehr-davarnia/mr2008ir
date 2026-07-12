using Atelier.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class PageListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public PageStatus Status { get; set; }
}

public class PageEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "عنوان صفحه الزامی است.")]
    [Display(Name = "عنوان")]
    [StringLength(200, ErrorMessage = "عنوان حداکثر ۲۰۰ کاراکتر است.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "اسلاگ")]
    [StringLength(200, ErrorMessage = "اسلاگ حداکثر ۲۰۰ کاراکتر است.")]
    public string? Slug { get; set; }

    [Display(Name = "محتوا")]
    public string? Content { get; set; }

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
