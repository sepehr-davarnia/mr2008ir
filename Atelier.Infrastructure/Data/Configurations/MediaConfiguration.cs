using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.HasKey(media => media.Id);
        builder.Property(media => media.Id).ValueGeneratedOnAdd();

        builder.Property(media => media.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(media => media.Slug)
            .IsUnique();

        builder.Property(media => media.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(media => media.StorageId);

        builder.Property(media => media.ContentType)
            .HasMaxLength(128);

        builder.Property(media => media.FileSize);

        builder.Property(media => media.Title)
            .HasMaxLength(200);

        builder.Property(media => media.AltText)
            .HasMaxLength(300);

        builder.Property(media => media.FileName)
            .HasMaxLength(255);

        builder.Property(media => media.Purpose)
            .HasMaxLength(100);

        builder.Property(media => media.SourceUrl)
            .HasMaxLength(2048);

        builder.Property(media => media.IsNotDownloaded);

        builder.Property(media => media.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(media => media.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");
    }
}
