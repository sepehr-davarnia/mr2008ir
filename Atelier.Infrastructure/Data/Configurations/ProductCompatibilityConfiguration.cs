using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atelier.Infrastructure.Data.Configurations;

public class ProductCompatibilityConfiguration : IEntityTypeConfiguration<ProductCompatibility>
{
    public void Configure(EntityTypeBuilder<ProductCompatibility> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Note).HasMaxLength(500);
        builder.HasIndex(item => new { item.ProductId, item.VehicleId }).IsUnique();
        builder.Property(item => item.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(item => item.UpdatedAt).HasColumnType("datetime2");
        builder.HasOne(item => item.Vehicle).WithMany().HasForeignKey(item => item.VehicleId).OnDelete(DeleteBehavior.Cascade);
    }
}
