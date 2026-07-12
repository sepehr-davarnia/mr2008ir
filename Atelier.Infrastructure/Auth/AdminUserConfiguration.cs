using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atelier.Infrastructure.Auth;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
        new(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUsers");
        builder.HasKey(adminUser => adminUser.Id);
        builder.Property(adminUser => adminUser.Id).ValueGeneratedOnAdd();

        builder.Property(adminUser => adminUser.Username)
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(adminUser => adminUser.PasswordHash)
            .HasMaxLength(1024)
            .IsRequired();
        builder.Property(adminUser => adminUser.IsActive)
            .IsRequired();
        builder.Property(adminUser => adminUser.CreatedAt)
            .HasConversion(UtcConverter)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(adminUser => adminUser.Username)
            .IsUnique();

        var createdAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const string passwordHash = "AQAAAAEAACcQAAAAEAECAwQFBgcICQoLDA0ODxBInfghqg410IjfSdWE/uHjV1trfVCn4IYFiHLptjvZOg==";

        builder.HasData(new
        {
            Id = 1,
            Username = "admin",
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = createdAt
        });
    }
}
