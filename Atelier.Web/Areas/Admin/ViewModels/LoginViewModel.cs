using System.ComponentModel.DataAnnotations;

namespace Atelier.Web.Areas.Admin.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    [StringLength(256)]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [StringLength(256)]
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
