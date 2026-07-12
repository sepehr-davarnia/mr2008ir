using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Data.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
        new(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(lead => lead.Id);
        builder.Property(lead => lead.Id).ValueGeneratedOnAdd();

        builder.Property(lead => lead.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(lead => lead.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(lead => lead.Message)
            .HasMaxLength(2000);

        builder.Property(lead => lead.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(lead => lead.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(lead => lead.UpdatedAt)
            .HasConversion(UtcNullableConverter)
            .HasColumnType("datetime2");
    }
}
