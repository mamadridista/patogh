using Microsoft.EntityFrameworkCore;
using Patogh.Domain.Entities;
using Patogh.Domain.Enums;
using Patogh.Persistence.Context;

namespace Patogh.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        // Password for all test users: Test@1234
        // Hash generated with BCrypt work factor 11
        const string passwordHash =
            "$2a$11$3QxDjD1ylgPnRgQLhBrTaeTd5I1Bdab00vBGCMQcMkjRXBtHHFViq";

        // ── Users ─────────────────────────────────────────────────────────────
        var adminId = Guid.NewGuid();
        var owner1Id = Guid.NewGuid();
        var owner2Id = Guid.NewGuid();
        var customer1Id = Guid.NewGuid();
        var customer2Id = Guid.NewGuid();

        var users = new List<User>
        {
            new() { Id = adminId,     PhoneNumber = "09000000000", PasswordHash = passwordHash, Role = UserRole.Admin },
            new() { Id = owner1Id,    PhoneNumber = "09111111111", PasswordHash = passwordHash, Role = UserRole.RestaurantOwner },
            new() { Id = owner2Id,    PhoneNumber = "09222222222", PasswordHash = passwordHash, Role = UserRole.RestaurantOwner },
            new() { Id = customer1Id, PhoneNumber = "09333333333", PasswordHash = passwordHash, Role = UserRole.Customer },
            new() { Id = customer2Id, PhoneNumber = "09444444444", PasswordHash = passwordHash, Role = UserRole.Customer }
        };
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // ── Restaurants ───────────────────────────────────────────────────────
        var rest1Id = Guid.NewGuid();
        var rest2Id = Guid.NewGuid();

        var restaurants = new List<Restaurant>
        {
            new()
            {
                Id = rest1Id,
                Name = "پاتوق کافه",
                Description = "یک کافه دنج و صمیمی در قلب شهر",
                Location = "تهران، خیابان ولیعصر",
                PriceRange = "متوسط",
                FoodType = "کافه",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime   = new TimeSpan(23, 0, 0),
                OwnerId   = owner1Id,
                IsApproved = true
            },
            new()
            {
                Id = rest2Id,
                Name = "رستوران سنتی ایران",
                Description = "غذاهای اصیل ایرانی با طعم خانگی",
                Location = "تهران، خیابان انقلاب",
                PriceRange = "گران",
                FoodType = "ایرانی",
                StartTime = new TimeSpan(11, 0, 0),
                EndTime   = new TimeSpan(22, 0, 0),
                OwnerId   = owner2Id,
                IsApproved = true
            }
        };
        await context.Restaurants.AddRangeAsync(restaurants);
        await context.SaveChangesAsync();

        // ── Menu Items ────────────────────────────────────────────────────────
        var menuItems = new List<MenuItem>
        {
            new() { RestaurantId = rest1Id, Name = "قهوه ترک",      Description = "قهوه ترک اصیل",        Price = 45000 },
            new() { RestaurantId = rest1Id, Name = "کاپوچینو",       Description = "با شیر تازه",          Price = 65000 },
            new() { RestaurantId = rest1Id, Name = "چیزکیک",         Description = "چیزکیک خانگی",        Price = 85000 },
            new() { RestaurantId = rest2Id, Name = "چلو کباب کوبیده", Description = "با برنج ایرانی",      Price = 180000 },
            new() { RestaurantId = rest2Id, Name = "قورمه سبزی",     Description = "با برنج و سالاد",      Price = 165000 },
            new() { RestaurantId = rest2Id, Name = "جوجه کباب",      Description = "با زعفران و گوجه",    Price = 200000 }
        };
        await context.MenuItems.AddRangeAsync(menuItems);

        // ── Tables ────────────────────────────────────────────────────────────
        var table1 = Guid.NewGuid(); var table2 = Guid.NewGuid(); var table3 = Guid.NewGuid();
        var table4 = Guid.NewGuid(); var table5 = Guid.NewGuid(); var table6 = Guid.NewGuid();

        var tables = new List<RestaurantTable>
        {
            new() { Id = table1, RestaurantId = rest1Id, TableNumber = 1, Capacity = 2 },
            new() { Id = table2, RestaurantId = rest1Id, TableNumber = 2, Capacity = 4 },
            new() { Id = table3, RestaurantId = rest1Id, TableNumber = 3, Capacity = 6 },
            new() { Id = table4, RestaurantId = rest2Id, TableNumber = 1, Capacity = 4 },
            new() { Id = table5, RestaurantId = rest2Id, TableNumber = 2, Capacity = 4 },
            new() { Id = table6, RestaurantId = rest2Id, TableNumber = 3, Capacity = 8 }
        };
        await context.RestaurantTables.AddRangeAsync(tables);
        await context.SaveChangesAsync();

        // ── Seed Reservations (for testing the UI) ────────────────────────────
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tomorrow = today.AddDays(1);

        var reservations = new List<Reservation>
        {
            // Confirmed reservation for customer1 at café, today
            new()
            {
                CustomerId = customer1Id, RestaurantId = rest1Id, TableId = table2,
                CustomerName = "علی رضایی", CustomerPhone = "09333333333",
                GuestCount = 3, ReservationDate = today,
                StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(16, 0, 0),
                Status = ReservationStatus.Confirmed, Notes = "لطفاً میز کنار پنجره باشد"
            },
            // Pending reservation for customer2 at Iranian restaurant, tomorrow
            new()
            {
                CustomerId = customer2Id, RestaurantId = rest2Id, TableId = table4,
                CustomerName = "سارا محمدی", CustomerPhone = "09444444444",
                GuestCount = 2, ReservationDate = tomorrow,
                StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(15, 0, 0),
                Status = ReservationStatus.Pending
            },
            // Another booking at café tomorrow — blocks table3 from 19:00-21:00
            new()
            {
                CustomerId = customer1Id, RestaurantId = rest1Id, TableId = table3,
                CustomerName = "علی رضایی", CustomerPhone = "09333333333",
                GuestCount = 5, ReservationDate = tomorrow,
                StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(21, 0, 0),
                Status = ReservationStatus.Confirmed
            }
        };
        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
    }
}