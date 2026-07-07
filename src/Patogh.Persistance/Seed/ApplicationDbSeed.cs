using Microsoft.EntityFrameworkCore;
using Patogh.Domain.Entities;
using Patogh.Domain.Enums;
using Patogh.Persistence.Context;

namespace Patogh.Persistence.Seed;

public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var adminUser = new User
        {
            PhoneNumber = "09120000000",
            PasswordHash = "ADMIN_HASH",
            Role = UserRole.Admin
        };

        var customerUser = new User
        {
            PhoneNumber = "09121111111",
            PasswordHash = "CUSTOMER_HASH",
            Role = UserRole.Customer
        };

        await context.Users.AddRangeAsync(adminUser, customerUser);

        var restaurant = new Restaurant
        {
            Name = "Patogh Cafe",
            Description = "Sample restaurant for development",
            Location = "Tehran",
            PriceRange = "Medium",
            FoodType = "Cafe",
            OwnerId = adminUser.Id,
            IsApproved = true
        };
        await context.Restaurants.AddAsync(restaurant);

        var menuItems = new List<MenuItem>
        {
            new()
            {
                RestaurantId = restaurant.Id,
                Name = "Espresso",
                Description = "Italian coffee",
                Price = 120000
            },
            new()
            {
                RestaurantId = restaurant.Id,
                Name = "Cheese Burger",
                Description = "Classic burger",
                Price = 350000
            }
        };
        await context.MenuItems.AddRangeAsync(menuItems);

        var tables = new List<RestaurantTable>
        {
            new()
            {
                RestaurantId = restaurant.Id,
                TableNumber = 1,
                Capacity = 2
            },
            new()
            {
                RestaurantId = restaurant.Id,
                TableNumber = 2,
                Capacity = 4
            }
        };

        await context.RestaurantTables.AddRangeAsync(tables);

        await context.SaveChangesAsync();
    }
}