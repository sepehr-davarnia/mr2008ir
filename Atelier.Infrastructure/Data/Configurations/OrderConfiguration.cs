using Atelier.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atelier.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Number).HasMaxLength(32).IsRequired();
        builder.HasIndex(order => order.Number).IsUnique();
        builder.Property(order => order.CustomerName).HasMaxLength(120).IsRequired();
        builder.Property(order => order.Phone).HasMaxLength(20).IsRequired();
        builder.Property(order => order.Province).HasMaxLength(80).IsRequired();
        builder.Property(order => order.City).HasMaxLength(80).IsRequired();
        builder.Property(order => order.Address).HasMaxLength(500).IsRequired();
        builder.Property(order => order.PostalCode).HasMaxLength(10);
        builder.Property(order => order.CustomerNote).HasMaxLength(1000);
        builder.Property(order => order.Total).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(order => order.Status).HasConversion<int>().HasDefaultValue(OrderStatus.AwaitingReview).IsRequired();
        builder.Property(order => order.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(order => order.UpdatedAt).HasColumnType("datetime2");
        builder.HasMany(order => order.Items).WithOne().HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
