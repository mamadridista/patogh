using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patogh.Domain.Entities;

namespace Patogh.Persistence.Configurations;

public class RestaurantImageConfiguration : IEntityTypeConfiguration<RestaurantImage>
{
    public void Configure(EntityTypeBuilder<RestaurantImage> builder)
    {
        builder.ToTable("RestaurantImages");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Restaurant)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MediaAsset)
            .WithMany()
            .HasForeignKey(x => x.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}