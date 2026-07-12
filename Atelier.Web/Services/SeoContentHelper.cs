using System;
using System.Collections.Generic;
using System.Linq;
using Atelier.Web.ViewModels;

namespace Atelier.Web.Services;

public static class SeoContentHelper
{
    public static string? BuildMetaDescription(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var normalized = content.Replace("\n", " ").Replace("\r", " ").Trim();
        return normalized.Length <= 160 ? normalized : normalized[..160];
    }

    public static string? BuildShortDescription(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var normalized = content.Replace("\n", " ").Replace("\r", " ").Trim();
        return normalized.Length <= 120 ? normalized : normalized[..120];
    }

    public static string? ExtractDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var markerIndex = description.IndexOf("مشخصات فنی:", StringComparison.Ordinal);
        if (markerIndex <= 0)
        {
            return description;
        }

        return description[..markerIndex].Trim();
    }

    public static IReadOnlyList<ProductSpecificationViewModel> ParseSpecifications(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Array.Empty<ProductSpecificationViewModel>();
        }

        var markerIndex = description.IndexOf("مشخصات فنی:", StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return Array.Empty<ProductSpecificationViewModel>();
        }

        var specsSection = description[(markerIndex + "مشخصات فنی:".Length)..];
        var stopIndex = specsSection.IndexOf("دسته بندی:", StringComparison.Ordinal);
        if (stopIndex >= 0)
        {
            specsSection = specsSection[..stopIndex];
        }

        var segments = specsSection
            .Split(new[] { '،', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();

        var results = new List<ProductSpecificationViewModel>();
        foreach (var segment in segments)
        {
            var parts = segment.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                results.Add(new ProductSpecificationViewModel
                {
                    Label = parts[0],
                    Value = parts[1]
                });
            }
            else
            {
                results.Add(new ProductSpecificationViewModel
                {
                    Label = "مشخصات",
                    Value = segment
                });
            }
        }

        return results;
    }
}
