using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class CategoryListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CategoryEditViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "عنوان دسته‌بندی الزامی است.")]
    [Display(Name = "عنوان دسته‌بندی")]
    [StringLength(200, ErrorMessage = "عنوان دسته‌بندی حداکثر ۲۰۰ کاراکتر است.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسلاگ الزامی است.")]
    [Display(Name = "اسلاگ")]
    [StringLength(200, ErrorMessage = "اسلاگ حداکثر ۲۰۰ کاراکتر است.")]
    public string? Slug { get; set; }

    [Display(Name = "تصویر دسته‌بندی")]
    public int? MediaId { get; set; }

    public string? MediaPreviewUrl { get; set; }
}
