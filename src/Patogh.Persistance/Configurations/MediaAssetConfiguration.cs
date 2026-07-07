using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patogh.Domain.Entities;

namespace Patogh.Persistence.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("MediaAssets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName).HasMaxLength(250);
        builder.Property(x => x.FilePath).HasMaxLength(1000);
        builder.Property(x => x.ContentType).HasMaxLength(100);
    }
}