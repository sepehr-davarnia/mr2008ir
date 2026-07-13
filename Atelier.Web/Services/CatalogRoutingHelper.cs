using System;
using System.Collections.Generic;
using System.Linq;

namespace Atelier.Web.Services;

public static class CatalogRoutingHelper
{

    public static IReadOnlyList<CatalogCategoryNode> BuildCategoryChain(
        IReadOnlyList<CatalogCategoryNode> categories,
        CatalogCategoryNode category)
    {
        var chain = new List<CatalogCategoryNode> { category };
        var current = category;

        while (current.ParentId.HasValue)
        {
            var parent = categories.FirstOrDefault(item => item.Id == current.ParentId.Value);
            if (parent is null)
            {
                break;
            }

            chain.Insert(0, parent);
            current = parent;
        }

        return chain;
    }

    public static string BuildCategoryPath(IReadOnlyList<CatalogCategoryNode> categoryChain)
    {
        var segments = new List<string>();
        string? parentSlug = null;

        foreach (var node in categoryChain)
        {
            var segment = GetSegment(node.Slug, parentSlug);
            segments.Add(segment);
            parentSlug = node.Slug;
        }

        return "/" + string.Join('/', segments);
    }

    public static string BuildCategoryPath(IReadOnlyList<CatalogCategoryNode> categories, CatalogCategoryNode category)
    {
        var chain = BuildCategoryChain(categories, category);
        return BuildCategoryPath(chain);
    }

    public static string BuildProductPath(IReadOnlyList<CatalogCategoryNode> categoryChain, string productSlug)
    {
        var basePath = BuildCategoryPath(categoryChain);
        return $"{basePath}/{productSlug}";
    }

    public static string GetSegment(string slug, string? parentSlug)
    {
        if (!string.IsNullOrWhiteSpace(parentSlug)
            && slug.StartsWith(parentSlug + "-", StringComparison.OrdinalIgnoreCase))
        {
            return slug[(parentSlug.Length + 1)..];
        }

        return slug;
    }

    public static bool IsSegmentMatch(string slug, string segment, string? parentSlug)
    {
        if (slug.Equals(segment, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(parentSlug)
               && slug.StartsWith(parentSlug + "-", StringComparison.OrdinalIgnoreCase)
               && slug.EndsWith("-" + segment, StringComparison.OrdinalIgnoreCase);
    }

}

public sealed class CatalogCategoryNode
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public int? MediaId { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
