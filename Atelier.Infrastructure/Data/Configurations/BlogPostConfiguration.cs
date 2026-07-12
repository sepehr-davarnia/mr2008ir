using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.HasKey(post => post.Id);
        builder.Property(post => post.Id).ValueGeneratedOnAdd();

        builder.Property(post => post.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(post => post.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(post => post.Slug)
            .IsUnique();

        builder.Property(post => post.Excerpt)
            .HasMaxLength(600)
            .IsRequired();

        builder.Property(post => post.Content)
            .IsRequired();

        builder.Property(post => post.MetaTitle)
            .HasMaxLength(200);

        builder.Property(post => post.MetaDescription)
            .HasMaxLength(300);

        builder.Property(post => post.PublishedAt)
            .HasColumnType("datetimeoffset");

        builder.Property(post => post.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(post => post.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");

        builder.ToTable("BlogPosts");
    }
}
