using System.Globalization;
using System.Text;

namespace Atelier.Web.Services;

public static class PersianSearchNormalizer
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var builder = new StringBuilder(value.Length);
        foreach (var raw in value.Trim().Normalize(NormalizationForm.FormKC))
        {
            var character = raw switch
            {
                'ي' or 'ى' => 'ی',
                'ك' => 'ک',
                'ۀ' or 'ة' => 'ه',
                '‌' => ' ',
                >= '۰' and <= '۹' => (char)('0' + raw - '۰'),
                >= '٠' and <= '٩' => (char)('0' + raw - '٠'),
                _ => raw
            };
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                builder.Append(char.IsWhiteSpace(character) ? ' ' : char.ToLowerInvariant(character));
        }
        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string CompactPartNumber(string value) =>
        new(Normalize(value).Where(char.IsLetterOrDigit).ToArray());
}
