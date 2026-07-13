using System.Security.Claims;
using Atelier.Web.Areas.Admin.ViewModels;
using Atelier.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthController : Controller
{
    private readonly IAdminAuthService _adminAuthService;

    public AuthController(IAdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("admin-login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var adminUser = await _adminAuthService.AuthenticateAsync(
            model.UserName ?? string.Empty,
            model.Password ?? string.Empty);

        if (adminUser is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, adminUser.Username)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth", new { area = "Admin" });
    }
}
