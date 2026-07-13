using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Atelier.Domain.Enums;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class ProductIndexViewModel
{
    public string? Search { get; set; }
    public IReadOnlyList<ProductListItemViewModel> Products { get; set; } = Array.Empty<ProductListItemViewModel>();
}

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
}

public abstract class ProductFormViewModel
{
    [Required(ErrorMessage = "نام محصول الزامی است.")]
    [Display(Name = "نام محصول")]
    [StringLength(200, ErrorMessage = "نام محصول حداکثر ۲۰۰ کاراکتر است.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "اسلاگ")]
    [StringLength(200, ErrorMessage = "اسلاگ حداکثر ۲۰۰ کاراکتر است.")]
    public string? Slug { get; set; }

    [Display(Name = "توضیح")]
    [StringLength(1000, ErrorMessage = "توضیح حداکثر ۱۰۰۰ کاراکتر است.")]
    public string? Description { get; set; }

    [Display(Name = "وضعیت")]
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    [Display(Name = "قیمت (تومان)")]
    [Range(0, 1_000_000_000, ErrorMessage = "قیمت نمی‌تواند منفی باشد.")]
    public decimal? Price { get; set; }

    [Display(Name = "برند")]
    [StringLength(120)]
    public string? Brand { get; set; }

    [Display(Name = "سازنده")]
    [StringLength(160)]
    public string? Manufacturer { get; set; }

    [Display(Name = "شماره OEM")]
    [StringLength(120)]
    public string? OemPartNumber { get; set; }

    [Display(Name = "کد فنی")]
    [StringLength(120)]
    public string? TechnicalPartNumber { get; set; }

    [Display(Name = "کدهای جایگزین")]
    [StringLength(500)]
    public string? AlternatePartNumbers { get; set; }

    [Display(Name = "دسته‌بندی‌ها")]
    public List<int> CategoryIds { get; set; } = new();

    [Display(Name = "خودروهای سازگار")]
    public List<int> VehicleIds { get; set; } = new();

    [Display(Name = "نیاز به تأیید VIN")]
    public bool RequiresVinCheck { get; set; }

    public IReadOnlyList<SelectOptionViewModel> CategoryOptions { get; set; } = Array.Empty<SelectOptionViewModel>();
    public IReadOnlyList<SelectOptionViewModel> VehicleOptions { get; set; } = Array.Empty<SelectOptionViewModel>();
}

public class SelectOptionViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class ProductCreateViewModel : ProductFormViewModel
{
}

public class ProductEditViewModel : ProductFormViewModel
{
    public int Id { get; set; }
    public IReadOnlyList<MediaItemViewModel> Gallery { get; set; } = Array.Empty<MediaItemViewModel>();
}

public class MediaItemViewModel
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? AltText { get; set; }
    public string? ContentType { get; set; }
}

public class ProductPricingIndexViewModel
{
    public IReadOnlyList<ProductPriceItemViewModel> Products { get; set; } = Array.Empty<ProductPriceItemViewModel>();
}

public class ProductPriceItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
    public decimal? Price { get; set; }
    public PriceType PriceType { get; set; }
    public string PriceDisplay { get; set; } = string.Empty;
}

public class ProductPriceUpdateViewModel
{
    [Required]
    public int Id { get; set; }

    [Display(Name = "قیمت (تومان)")]
    [Range(0, 1000000000, ErrorMessage = "قیمت نمی‌تواند منفی باشد.")]
    public decimal? Price { get; set; }

    public bool IsContactPrice { get; set; }
}
