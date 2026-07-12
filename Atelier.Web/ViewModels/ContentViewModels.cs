using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.ViewModels;

public class PageViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = new List<BreadcrumbItemViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

public class ProjectListViewModel
{
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = new List<BreadcrumbItemViewModel>();
    public IReadOnlyList<ProjectListItemViewModel> Projects { get; set; } = new List<ProjectListItemViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

public class ProjectListItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
}

public class ProjectDetailViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? HeroImageAltText { get; set; }
    public IReadOnlyList<MediaItemViewModel> Gallery { get; set; } = new List<MediaItemViewModel>();
    public IReadOnlyList<BreadcrumbItemViewModel> Breadcrumbs { get; set; } = new List<BreadcrumbItemViewModel>();
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string CanonicalUrl { get; set; } = string.Empty;
}

public class ContactFormViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
    public IReadOnlyList<FormFieldViewModel> Fields { get; set; } = new List<FormFieldViewModel>();
}

public class FormFieldViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; }
}

public class ContactPageViewModel : PageViewModel
{
    public ContactFormViewModel Form { get; set; } = new();
    public bool Submitted { get; set; }
    public ContactFormInputModel FormInput { get; set; } = new();
}

public class ContactFormInputModel
{
    [Required(ErrorMessage = "نام خود را وارد کنید.")]
    [StringLength(200, ErrorMessage = "نام حداکثر ۲۰۰ کاراکتر است.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل را وارد کنید.")]
    [EmailAddress(ErrorMessage = "ایمیل وارد شده معتبر نیست.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "شماره تماس حداکثر ۲۰ کاراکتر است.")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "پیام خود را بنویسید.")]
    [StringLength(2000, ErrorMessage = "پیام حداکثر ۲۰۰۰ کاراکتر است.")]
    public string Message { get; set; } = string.Empty;
}
