using Microsoft.EntityFrameworkCore;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;
using System.Reflection;

namespace Patogh.Persistence.Context;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<RestaurantImage> RestaurantImages => Set<RestaurantImage>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public new Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database
        => base.Database;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global soft delete filters
        modelBuilder.Entity<Restaurant>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<MenuItem>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RestaurantTable>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Reservation>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<MediaAsset>()
            .HasQueryFilter(x => !x.IsDeleted);
    }
}