using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<SiteSetting> builder)
    {
        builder.HasKey(setting => setting.Id);
        builder.Property(setting => setting.Id).ValueGeneratedOnAdd();

        builder.Property(setting => setting.Key)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(setting => setting.Key)
            .IsUnique();

        builder.Property(setting => setting.Value)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(setting => setting.SiteName)
            .HasMaxLength(200);

        builder.Property(setting => setting.Address)
            .HasMaxLength(500);

        builder.Property(setting => setting.Phone)
            .HasMaxLength(100);

        builder.Property(setting => setting.Mobile)
            .HasMaxLength(100);

        builder.Property(setting => setting.WhatsApp)
            .HasMaxLength(200);

        builder.Property(setting => setting.Instagram)
            .HasMaxLength(200);

        builder.Property(setting => setting.Telegram)
            .HasMaxLength(200);

        builder.Property(setting => setting.Email)
            .HasMaxLength(200);

        builder.Property(setting => setting.LogoMediaId);

        builder.Property(setting => setting.FaviconMediaId);

        builder.Property(setting => setting.HomeHeroMediaId);

        builder.Property(setting => setting.HomeSecondaryMediaId);

        builder.Property(setting => setting.DefaultCategoryMediaId);

        builder.Property(setting => setting.MaxUploadSizeKb)
            .HasDefaultValue(5120)
            .IsRequired();

        builder.Property(setting => setting.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(setting => setting.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");
    }
}
