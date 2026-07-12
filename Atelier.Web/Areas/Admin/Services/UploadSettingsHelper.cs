using Atelier.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Services;

public static class UploadSettingsHelper
{
    public const int DefaultMaxUploadSizeKb = 5120;

    public static async Task<SiteSetting> GetOrCreateSettingsAsync(AtelierDbContext dbContext)
    {
        var settings = await dbContext.SiteSettings
            .OrderBy(item => item.Id)
            .ToListAsync();

        if (settings.Count == 0)
        {
            var created = new SiteSetting(DefaultMaxUploadSizeKb);
            dbContext.SiteSettings.Add(created);
            await dbContext.SaveChangesAsync();
            return created;
        }

        var primary = settings[0];
        var normalizedSize = NormalizeMaxUploadSizeKb(primary.MaxUploadSizeKb);

        if (normalizedSize != primary.MaxUploadSizeKb)
        {
            primary.UpdateMaxUploadSizeKb(normalizedSize);
        }

        if (settings.Count > 1)
        {
            dbContext.SiteSettings.RemoveRange(settings.Skip(1));
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync();
        }

        return primary;
    }

    public static int NormalizeMaxUploadSizeKb(int maxUploadSizeKb)
    {
        return maxUploadSizeKb > 0 ? maxUploadSizeKb : DefaultMaxUploadSizeKb;
    }

    public static long ToBytes(int maxUploadSizeKb)
    {
        return (long)maxUploadSizeKb * 1024L;
    }

    public static string FormatMegabytes(int maxUploadSizeKb)
    {
        var megabytes = maxUploadSizeKb / 1024m;
        return megabytes % 1 == 0
            ? megabytes.ToString("0")
            : megabytes.ToString("0.#");
    }
}
