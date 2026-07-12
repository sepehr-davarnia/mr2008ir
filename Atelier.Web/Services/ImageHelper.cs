using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace Atelier.Web.Services;

public static class ImageHelper
{
    private static readonly IReadOnlyList<int> HeroBreakpointValues = new[] { 640, 960, 1280, 1600, 1920 };
    private static readonly IReadOnlyList<int> FeatureBreakpointValues = new[] { 320, 480, 720, 1024, 1280 };
    private static readonly IReadOnlyList<int> GalleryBreakpointValues = new[] { 360, 540, 720, 960, 1280 };
    private static readonly IReadOnlyList<int> LogoBreakpointValues = new[] { 120, 200, 320, 480 };

    public static IReadOnlyList<int> HeroBreakpoints => HeroBreakpointValues;
    public static IReadOnlyList<int> FeatureBreakpoints => FeatureBreakpointValues;
    public static IReadOnlyList<int> GalleryBreakpoints => GalleryBreakpointValues;
    public static IReadOnlyList<int> LogoBreakpoints => LogoBreakpointValues;

    public static string? BuildResizedUrl(string? url, int? width = null, int? height = null, string? format = "webp")
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var parameters = new Dictionary<string, string>();
        if (width.HasValue)
        {
            parameters["width"] = width.Value.ToString();
        }

        if (height.HasValue)
        {
            parameters["height"] = height.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(format))
        {
            parameters["format"] = format;
        }

        return parameters.Count == 0 ? url : QueryHelpers.AddQueryString(url, parameters);
    }

    public static string BuildSrcSet(string? url, IEnumerable<int> widths, string? format = "webp")
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        var entries = widths
            .Distinct()
            .OrderBy(width => width)
            .Select(width =>
            {
                var resized = BuildResizedUrl(url, width, null, format);
                return string.IsNullOrWhiteSpace(resized) ? null : $"{resized} {width}w";
            })
            .Where(entry => entry is not null);

        return string.Join(", ", entries);
    }
}
