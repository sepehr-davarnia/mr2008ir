using System;
using System.Globalization;
using Atelier.Domain.Enums;

namespace Atelier.Web.Services;

public static class LocalizationHelper
{
    private static readonly Lazy<CultureInfo> LazyPersianCulture = new(CreatePersianCulture);
    public const string ContactForPriceText = "جهت اطلاع از قیمت تماس بگیرید";

    public static CultureInfo PersianCulture => LazyPersianCulture.Value;

    public static string FormatDate(DateTimeOffset? date, string format = "yyyy/MM/dd")
    {
        if (date is null)
        {
            return string.Empty;
        }

        return date.Value.ToLocalTime().ToString(format, PersianCulture);
    }

    public static string FormatDate(DateTime date, string format = "yyyy/MM/dd")
    {
        var adjusted = date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc).ToLocalTime()
            : date.ToLocalTime();

        return adjusted.ToString(format, PersianCulture);
    }

    public static string FormatPrice(decimal? price, PriceType priceType = PriceType.Fixed)
    {
        if (priceType == PriceType.Contact || price is null || price <= 0)
        {
            return ContactForPriceText;
        }

        return string.Format(PersianCulture, "{0:N0} تومان", price.Value);
    }

    private static CultureInfo CreatePersianCulture()
    {
        var culture = new CultureInfo("fa-IR");
        culture.DateTimeFormat.Calendar = new PersianCalendar();
        culture.NumberFormat.CurrencySymbol = "تومان";
        return culture;
    }
}
