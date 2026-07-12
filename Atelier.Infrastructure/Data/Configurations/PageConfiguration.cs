using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasKey(page => page.Id);
        builder.Property(page => page.Id).ValueGeneratedOnAdd();

        builder.Property(page => page.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(page => page.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(page => page.Slug)
            .IsUnique();

        builder.Property(page => page.Content)
            .HasMaxLength(8000);

        builder.Property(page => page.FeaturedMediaId);

        builder.Property(page => page.MetaTitle)
            .HasMaxLength(200);

        builder.Property(page => page.MetaDescription)
            .HasMaxLength(300);

        builder.Property(page => page.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(page => page.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(page => page.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");

        builder.HasMany<Tag>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "PageTag",
                right => right.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                left => left.HasOne<Page>().WithMany().HasForeignKey("PageId"),
                join =>
                {
                    join.HasKey("PageId", "TagId");
                    join.HasIndex("TagId");
                });
    }
}
