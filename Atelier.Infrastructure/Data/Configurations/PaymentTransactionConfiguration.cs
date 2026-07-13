using Atelier.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atelier.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Gateway).HasMaxLength(40).IsRequired();
        builder.Property(item => item.Authority).HasMaxLength(80);
        builder.Property(item => item.ReferenceId).HasMaxLength(80);
        builder.Property(item => item.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(item => item.Status).HasConversion<int>().HasDefaultValue(PaymentStatus.Pending);
        builder.Property(item => item.FailureReason).HasMaxLength(500);
        builder.HasIndex(item => item.Authority).IsUnique().HasFilter("[Authority] IS NOT NULL");
        builder.HasIndex(item => item.OrderId);
        builder.Property(item => item.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(item => item.UpdatedAt).HasColumnType("datetime2");
        builder.HasOne<Order>().WithMany().HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
