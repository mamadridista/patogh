using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patogh.Domain.Entities;

namespace Patogh.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PriceRange)
            .HasMaxLength(100);

        builder.Property(x => x.FoodType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsApproved)
            .HasDefaultValue(false);

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Location);
        builder.HasIndex(x => x.FoodType);

        builder.HasOne(x => x.Owner)
            .WithMany(x => x.OwnedRestaurants)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.MenuItems)
            .WithOne(x => x.Restaurant)
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tables)
            .WithOne(x => x.Restaurant)
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CoverImage)
            .WithMany()
            .HasForeignKey(x => x.CoverImageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}