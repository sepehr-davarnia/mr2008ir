using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atelier.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Make).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Model).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Engine).HasMaxLength(80).IsRequired();
        builder.Property(item => item.Trim).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Slug).HasMaxLength(180).IsRequired();
        builder.HasIndex(item => item.Slug).IsUnique();
        builder.HasIndex(item => new { item.Make, item.Model, item.YearFrom, item.Engine, item.Trim });
        builder.Property(item => item.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(item => item.UpdatedAt).HasColumnType("datetime2");
    }
}
