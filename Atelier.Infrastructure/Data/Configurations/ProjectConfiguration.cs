using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Id).ValueGeneratedOnAdd();

        builder.Property(project => project.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(project => project.Slug)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(project => project.Slug)
            .IsUnique();

        builder.Property(project => project.Description)
            .HasMaxLength(2000);

        builder.Property(project => project.FeaturedMediaId);

        builder.Property(project => project.IsPublished)
            .IsRequired();

        builder.Property(project => project.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(project => project.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");

        builder.HasMany(project => project.Gallery)
            .WithOne()
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<Tag>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ProjectTag",
                right => right.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                left => left.HasOne<Project>().WithMany().HasForeignKey("ProjectId"),
                join =>
                {
                    join.HasKey("ProjectId", "TagId");
                    join.HasIndex("TagId");
                });
    }
}
