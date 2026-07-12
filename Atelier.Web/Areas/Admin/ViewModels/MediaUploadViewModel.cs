using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class MediaUploadViewModel
{
    public int ProductId { get; set; }
    public string? MaxUploadSizeDisplay { get; set; }

    [Display(Name = "فایل")]
    public IFormFile? File { get; set; }

    [Display(Name = "عنوان تصویر")]
    public string? Title { get; set; }

    [Display(Name = "متن جایگزین (Alt Text)")]
    public string? AltText { get; set; }
}
