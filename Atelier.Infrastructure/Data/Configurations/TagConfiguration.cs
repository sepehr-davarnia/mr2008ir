using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Id).ValueGeneratedOnAdd();

        builder.Property(tag => tag.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tag => tag.Slug)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(tag => tag.Slug)
            .IsUnique();

        builder.Property(tag => tag.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(tag => tag.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");
    }
}
