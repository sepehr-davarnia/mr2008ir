using System.Collections.Generic;

namespace Atelier.Web.ViewModels;

public class LayoutViewModel
{
    public string SiteName { get; set; } = "Atelier";
    public string Tagline { get; set; } = "چوب و ترمووود برای پروژه های معماری";
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public IReadOnlyList<NavigationLinkViewModel> PrimaryLinks { get; set; } = new List<NavigationLinkViewModel>();
    public IReadOnlyList<NavigationLinkViewModel> CategoryLinks { get; set; } = new List<NavigationLinkViewModel>();
    public IReadOnlyList<NavigationLinkViewModel> SocialLinks { get; set; } = new List<NavigationLinkViewModel>();
}

public class NavigationLinkViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class BreadcrumbItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
}
