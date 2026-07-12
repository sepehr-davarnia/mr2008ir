using System;

namespace Atelier.Web.Areas.Admin.Services;

public static class MediaAltTextHelper
{
    public const string RequiredMessage = "وارد کردن متن جایگزین تصویر (Alt Text) برای سئو الزامی است.";

    public static bool IsImageContentType(string? contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType)
            && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool HasAltText(string? altText)
    {
        return !string.IsNullOrWhiteSpace(altText);
    }
}
