using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Atelier.Web.Services;

public static class SeoHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string BuildCanonicalUrl(HttpRequest request, string? overrideUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(overrideUrl))
        {
            return overrideUrl;
        }

        var path = request.PathBase.Add(request.Path).ToString();
        return BuildAbsoluteUrl(request, path);
    }

    public static string BuildAbsoluteUrl(HttpRequest request, string path)
    {
        var host = request.Host.HasValue ? request.Host.Value : string.Empty;
        return string.Concat(request.Scheme, "://", host, path);
    }

    public static string BuildOrganizationSchema(SeoOrganizationSchemaData data)
    {
        return JsonSerializer.Serialize(new
        {
            @context = "https://schema.org",
            @type = "Organization",
            name = data.Name,
            url = data.Url,
            logo = data.LogoUrl,
            sameAs = data.SameAs.Count > 0 ? data.SameAs : null
        }, JsonOptions);
    }

    public static string BuildWebsiteSchema(SeoWebsiteSchemaData data)
    {
        return JsonSerializer.Serialize(new
        {
            @context = "https://schema.org",
            @type = "WebSite",
            name = data.Name,
            url = data.Url,
            inLanguage = "fa-IR",
            potentialAction = new
            {
                @type = "SearchAction",
                target = $"{data.Url}/categories?q={{search_term_string}}",
                queryInput = "required name=search_term_string"
            }
        }, JsonOptions);
    }

    public static string BuildPageSchema(SeoPageSchemaData data)
    {
        return data.Type switch
        {
            SeoPageSchemaType.BlogPost => JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type = "BlogPosting",
                headline = data.Title,
                description = data.Description,
                datePublished = data.PublishedAt?.ToString("O"),
                image = data.ImageUrl,
                mainEntityOfPage = data.CanonicalUrl
            }, JsonOptions),
            SeoPageSchemaType.WebPage => JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type = "WebPage",
                name = data.Title,
                description = data.Description,
                url = data.CanonicalUrl
            }, JsonOptions),
            SeoPageSchemaType.Project => JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type = "CreativeWork",
                name = data.Title,
                description = data.Description,
                image = data.ImageUrl,
                url = data.CanonicalUrl
            }, JsonOptions),
            SeoPageSchemaType.Product => JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type = "Product",
                name = data.Title,
                description = data.Description,
                image = data.ImageUrl,
                url = data.CanonicalUrl
            }, JsonOptions),
            _ => string.Empty
        };
    }

    public static string BuildProductSchema(SeoProductSchemaData data)
    {
        return JsonSerializer.Serialize(new
        {
            @context = "https://schema.org",
            @type = "Product",
            name = data.Name,
            description = data.Description,
            image = data.ImageUrl,
            url = data.CanonicalUrl,
            sku = data.Sku,
            mpn = data.Mpn,
            brand = string.IsNullOrWhiteSpace(data.Brand) ? null : new { @type = "Brand", name = data.Brand },
            offers = data.Price.HasValue
                ? new
                {
                    @type = "Offer",
                    price = data.Price.Value.ToString("0.##"),
                    priceCurrency = data.CurrencyCode ?? "IRR",
                    availability = data.AvailabilityUrl ?? "https://schema.org/InStock",
                    itemCondition = data.ItemConditionUrl ?? "https://schema.org/NewCondition",
                    url = data.CanonicalUrl,
                    seller = new { @type = "Organization", name = "mr2008.ir" },
                    hasMerchantReturnPolicy = string.IsNullOrWhiteSpace(data.ReturnPolicyUrl) ? null : new
                    {
                        @type = "MerchantReturnPolicy",
                        applicableCountry = "IR",
                        returnPolicyCategory = "https://schema.org/MerchantReturnUnspecified",
                        merchantReturnLink = data.ReturnPolicyUrl
                    }
                }
                : null
        }, JsonOptions);
    }

    public static string BuildBreadcrumbSchema(IReadOnlyList<SeoBreadcrumbItem> items)
    {
        if (items.Count == 0)
        {
            return string.Empty;
        }

        return JsonSerializer.Serialize(new
        {
            @context = "https://schema.org",
            @type = "BreadcrumbList",
            itemListElement = items.Select(item => new
            {
                @type = "ListItem",
                position = item.Position,
                name = item.Name,
                item = item.Url
            })
        }, JsonOptions);
    }
}

public sealed class SeoOrganizationSchemaData
{
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public List<string> SameAs { get; init; } = new();
}

public sealed class SeoWebsiteSchemaData
{
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}

public sealed class SeoPageSchemaData
{
    public SeoPageSchemaType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CanonicalUrl { get; init; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; init; }
    public string? ImageUrl { get; init; }
}

public enum SeoPageSchemaType
{
    BlogPost = 1,
    WebPage = 2,
    Project = 3,
    Product = 4
}

public sealed class SeoProductSchemaData
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CanonicalUrl { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal? Price { get; init; }
    public string? CurrencyCode { get; init; }
    public string? Sku { get; init; }
    public string? Mpn { get; init; }
    public string? Brand { get; init; }
    public string? AvailabilityUrl { get; init; }
    public string? ItemConditionUrl { get; init; }
    public string? ReturnPolicyUrl { get; init; }
}

public sealed class SeoBreadcrumbItem
{
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public int Position { get; init; }
}
