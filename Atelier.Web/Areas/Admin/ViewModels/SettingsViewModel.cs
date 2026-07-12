using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class SettingsViewModel
{
    [Display(Name = "نام سایت")]
    public string? SiteName { get; set; }

    [Display(Name = "آدرس")]
    public string? Address { get; set; }

    [Display(Name = "شماره تماس")]
    public string? Phone { get; set; }

    [Display(Name = "موبایل")]
    public string? Mobile { get; set; }

    [Display(Name = "واتساپ")]
    public string? WhatsApp { get; set; }

    [Display(Name = "اینستاگرام")]
    public string? Instagram { get; set; }

    [Display(Name = "تلگرام")]
    public string? Telegram { get; set; }

    [Display(Name = "ایمیل")]
    public string? Email { get; set; }

    [Display(Name = "لوگو")]
    public int? LogoMediaId { get; set; }

    [Display(Name = "فاوآیکن")]
    public int? FaviconMediaId { get; set; }

    [Display(Name = "تصویر هیرو صفحه اصلی")]
    public int? HomeHeroMediaId { get; set; }

    [Display(Name = "تصویر ثانویه صفحه اصلی")]
    public int? HomeSecondaryMediaId { get; set; }

    [Display(Name = "تصویر پیش فرض دسته بندی ها")]
    public int? DefaultCategoryMediaId { get; set; }

    public string? LogoPreviewUrl { get; set; }
    public string? FaviconPreviewUrl { get; set; }
    public string? HomeHeroPreviewUrl { get; set; }
    public string? HomeSecondaryPreviewUrl { get; set; }
    public string? DefaultCategoryPreviewUrl { get; set; }

    [Required(ErrorMessage = "حداکثر حجم مجاز فایل الزامی است.")]
    [Display(Name = "حداکثر حجم مجاز فایل (کیلوبایت)")]
    [Range(1, int.MaxValue, ErrorMessage = "حداکثر حجم مجاز فایل باید بیشتر از صفر باشد.")]
    public int? MaxUploadSizeKb { get; set; }
}
