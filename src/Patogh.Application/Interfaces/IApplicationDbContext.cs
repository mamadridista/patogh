using Microsoft.EntityFrameworkCore;
using Patogh.Domain.Entities;

namespace Patogh.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Restaurant> Restaurants { get; }
    DbSet<MenuItem> MenuItems { get; }
    DbSet<RestaurantTable> RestaurantTables { get; }
    DbSet<RestaurantImage> RestaurantImages { get; }
    DbSet<MediaAsset> MediaAssets { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<RefreshToken> RefreshTokens { get; }


    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}