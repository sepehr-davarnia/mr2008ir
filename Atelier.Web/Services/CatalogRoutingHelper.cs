using System;
using System.Collections.Generic;
using System.Linq;

namespace Atelier.Web.Services;

public static class CatalogRoutingHelper
{
    private static readonly Dictionary<string, IReadOnlyList<string>> CategoryProductMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["thermowood"] =
        [
            "thermowood-simple-19mm-42mm",
            "thermowood-simple-16mm",
            "thermowood-simple-14mm",
            "thermowood-simple-8mm",
            "thermowood-parvaneh-14mm"
        ],
        ["thermowood-tiles"] =
        [
            "thermowood-tile-standard",
            "thermowood-tile-puzzle"
        ],
        ["artificial-grass-tiles"] = ["artificial-grass-tile"],
        ["thermowall"] = ["thermowall-panel"],
        ["russian-wood"] =
        [
            "russian-wood-10x5-3m",
            "russian-wood-10x2-3m",
            "russian-wood-10x15-3m",
            "russian-wood-10x1-3m",
            "russian-wood-15x15-3m",
            "russian-wood-15x2-3m",
            "russian-wood-5x3-3m",
            "russian-wood-20x2-3m",
            "russian-wood-15x1-3m",
            "russian-wood-15x5-3m",
            "russian-wood-20x5-3m",
            "russian-bench-10x08-3m",
            "russian-bench-10x07-3m",
            "russian-angle-5cm",
            "russian-angle-3cm",
            "russian-wood-strip",
            "subwood-3x2",
            "subwood-4x2",
            "subwood-5x3",
            "subwood-4x6",
            "subwood-5x5"
        ],
        ["lambe"] = ["lambe-wood"]
    };

    public static bool TryGetPrimaryCategorySlug(string productSlug, out string categorySlug)
    {
        foreach (var entry in CategoryProductMap)
        {
            if (entry.Value.Contains(productSlug, StringComparer.OrdinalIgnoreCase))
            {
                categorySlug = entry.Key;
                return true;
            }
        }

        categorySlug = string.Empty;
        return false;
    }

    public static IReadOnlyCollection<string> GetProductSlugsForCategory(string categorySlug, IReadOnlyList<CatalogCategoryNode> categories)
    {
        var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var categorySlugs = GetDescendantSlugs(categorySlug, categories);
        categorySlugs.Add(categorySlug);

        foreach (var slug in categorySlugs)
        {
            if (!CategoryProductMap.TryGetValue(slug, out var products))
            {
                continue;
            }

            foreach (var productSlug in products)
            {
                slugs.Add(productSlug);
            }
        }

        return slugs;
    }

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

    private static HashSet<string> GetDescendantSlugs(string categorySlug, IReadOnlyList<CatalogCategoryNode> categories)
    {
        var descendants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var category = categories.FirstOrDefault(item => item.Slug.Equals(categorySlug, StringComparison.OrdinalIgnoreCase));
        if (category is null)
        {
            return descendants;
        }

        var queue = new Queue<CatalogCategoryNode>();
        queue.Enqueue(category);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = categories.Where(item => item.ParentId == current.Id).ToList();
            foreach (var child in children)
            {
                if (descendants.Add(child.Slug))
                {
                    queue.Enqueue(child);
                }
            }
        }

        return descendants;
    }
}

public sealed class CatalogCategoryNode
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public int? MediaId { get; init; }
}
