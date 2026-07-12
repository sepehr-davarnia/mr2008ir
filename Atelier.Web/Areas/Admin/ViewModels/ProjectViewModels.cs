using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class ProjectListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
}

public class ProjectEditViewModel
{
    public int? Id { get; set; }

    public IReadOnlyList<ProjectGalleryItemViewModel> Gallery { get; set; } = new List<ProjectGalleryItemViewModel>();

    public int? NewGalleryMediaId { get; set; }

    [Required(ErrorMessage = "عنوان پروژه الزامی است.")]
    [Display(Name = "عنوان پروژه")]
    [StringLength(200, ErrorMessage = "عنوان پروژه حداکثر ۲۰۰ کاراکتر است.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "اسلاگ")]
    [StringLength(200, ErrorMessage = "اسلاگ حداکثر ۲۰۰ کاراکتر است.")]
    public string? Slug { get; set; }

    [Display(Name = "توضیح")]
    [StringLength(2000, ErrorMessage = "توضیح حداکثر ۲۰۰۰ کاراکتر است.")]
    public string? Description { get; set; }

    [Display(Name = "تصویر شاخص")]
    public int? FeaturedMediaId { get; set; }

    public string? FeaturedMediaUrl { get; set; }

    [Display(Name = "وضعیت انتشار")]
    public bool IsPublished { get; set; }
}

public class ProjectGalleryItemViewModel
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
}
