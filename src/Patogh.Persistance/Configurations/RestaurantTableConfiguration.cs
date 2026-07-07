using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patogh.Domain.Entities;

namespace Patogh.Persistence.Configurations;

public class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{
    public void Configure(EntityTypeBuilder<RestaurantTable> builder)
    {
        builder.ToTable("RestaurantTables");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TableNumber).IsRequired();
        builder.Property(x => x.Capacity).IsRequired();

        builder.HasIndex(x => new { x.RestaurantId, x.TableNumber })
            .IsUnique();

        builder.HasOne(x => x.Restaurant)
            .WithMany(x => x.Tables)
            .HasForeignKey(x => x.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}