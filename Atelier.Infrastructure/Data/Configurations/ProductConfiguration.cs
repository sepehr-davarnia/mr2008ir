using Atelier.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedOnAdd();

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(product => product.Slug)
            .IsUnique();

        builder.Property(product => product.Description)
            .HasMaxLength(1000);

        builder.Property(product => product.Brand).HasMaxLength(120);
        builder.Property(product => product.Manufacturer).HasMaxLength(160);
        builder.Property(product => product.OemPartNumber).HasMaxLength(120);
        builder.Property(product => product.TechnicalPartNumber).HasMaxLength(120);
        builder.Property(product => product.AlternatePartNumbers).HasMaxLength(500);
        builder.HasIndex(product => product.OemPartNumber);
        builder.HasIndex(product => product.TechnicalPartNumber);

        builder.Property(product => product.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(product => product.Price)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(null)
            .IsRequired(false);

        builder.Property(product => product.PriceType)
            .HasConversion<int>()
            .HasDefaultValue(PriceType.Contact)
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(product => product.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");

        builder.HasMany(product => product.Gallery)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Tag>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ProductTag",
                right => right.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                left => left.HasOne<Product>().WithMany().HasForeignKey("ProductId"),
                join =>
                {
                    join.HasKey("ProductId", "TagId");
                    join.HasIndex("TagId");
                });

        builder.HasMany(product => product.Categories)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ProductCategory",
                right => right.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<Product>().WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("ProductId", "CategoryId");
                    join.HasIndex("CategoryId");
                });

        builder.HasMany(product => product.Compatibilities)
            .WithOne()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
