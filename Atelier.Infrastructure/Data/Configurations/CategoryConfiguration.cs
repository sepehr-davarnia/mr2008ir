using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedOnAdd();

        builder.Property(category => category.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(category => category.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(category => category.Slug)
            .IsUnique();

        builder.HasIndex(category => category.MediaId);

        builder.Property(category => category.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(category => category.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");

        builder.HasOne(category => category.Parent)
            .WithMany(category => category.Children)
            .HasForeignKey("ParentId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(category => category.MediaId);

        builder.HasOne<Media>()
            .WithMany()
            .HasForeignKey(category => category.MediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
