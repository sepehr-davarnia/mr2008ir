using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.Areas.Admin.ViewModels;

public sealed class VehicleEditViewModel
{
    public int? Id { get; set; }
    [Required, StringLength(80), Display(Name = "سازنده")] public string Make { get; set; } = string.Empty;
    [Required, StringLength(80), Display(Name = "مدل")] public string Model { get; set; } = string.Empty;
    [Range(1900, 2200), Display(Name = "سال شروع")] public int YearFrom { get; set; }
    [Range(1900, 2200), Display(Name = "سال پایان")] public int? YearTo { get; set; }
    [Required, StringLength(120), Display(Name = "موتور")] public string Engine { get; set; } = string.Empty;
    [Required, StringLength(120), Display(Name = "تریم")] public string Trim { get; set; } = string.Empty;
    [Required, StringLength(200), Display(Name = "اسلاگ")] public string Slug { get; set; } = string.Empty;
    [Display(Name = "فعال")] public bool IsActive { get; set; } = true;
}

public sealed class VehicleListItemViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}
