using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atelier.Infrastructure.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Status).HasConversion<int>().IsRequired();
        builder.Property(item => item.Note).HasMaxLength(500);
        builder.HasIndex(item => new { item.OrderId, item.CreatedAt });
        builder.Property(item => item.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(item => item.UpdatedAt).HasColumnType("datetime2");
    }
}
