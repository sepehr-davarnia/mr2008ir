using Atelier.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class LeadListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LeadDetailsViewModel
{
    public int Id { get; set; }

    [Display(Name = "نام")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "پیام")]
    public string? Message { get; set; }

    [Display(Name = "تاریخ ثبت")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "وضعیت")]
    public LeadStatus Status { get; set; }
}
