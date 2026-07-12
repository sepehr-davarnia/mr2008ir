using Atelier.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atelier.Infrastructure.Data.Configurations;

public class MediaContentConfiguration : IEntityTypeConfiguration<MediaContent>
{
    public void Configure(EntityTypeBuilder<MediaContent> builder)
    {
        builder.HasKey(content => content.Id);
        builder.Property(content => content.Id).ValueGeneratedOnAdd();

        builder.Property(content => content.Data)
            .IsRequired()
            .HasColumnType("varbinary(max)");

        builder.ToTable("MediaContents");
    }
}
