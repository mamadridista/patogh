using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Helpers;
using Patogh.Application.Features.Restaurants.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;

namespace Patogh.Application.Features.Admin.Queries.GetAdminStats;

public class GetAdminStatsQueryHandler
    : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminStatsDto> Handle(
        GetAdminStatsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateTimeHelper.TodayInTehran;
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        // ── Run all counts as parallel async tasks to minimise latency ────────
        // Each Task hits the DB independently. Total time = slowest single query,
        // not the sum of all queries.
        var venueStatsTask = _context.Restaurants
            .AsNoTracking()
            .IgnoreQueryFilters() // Include soft-deleted for admin audit purposes
            .GroupBy(r => new { r.IsApproved, r.IsDeleted })
            .Select(g => new
            {
                g.Key.IsApproved,
                g.Key.IsDeleted,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var newVenuesThisMonthTask = _context.Restaurants
            .AsNoTracking()
            .CountAsync(r => r.CreatedAt >= monthStart.ToDateTime(TimeOnly.MinValue),
                cancellationToken);

        var userStatsTask = _context.Users
            .AsNoTracking()
            .GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var newUsersThisMonthTask = _context.Users
            .AsNoTracking()
            .CountAsync(u => u.CreatedAt >= monthStart.ToDateTime(TimeOnly.MinValue),
                cancellationToken);

        var reservationStatsTask = _context.Reservations
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var reservationsThisMonthTask = _context.Reservations
            .AsNoTracking()
            .CountAsync(r => r.ReservationDate >= monthStart, cancellationToken);

        var topVenuesTask = _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.Status == ReservationStatus.Confirmed ||
                r.Status == ReservationStatus.Pending)
            .GroupBy(r => new { r.RestaurantId, r.Restaurant.Name, r.Restaurant.Location })
            .Select(g => new TopVenueDto
            {
                Id = g.Key.RestaurantId,
                Name = g.Key.Name,
                Location = g.Key.Location,
                ReservationCount = g.Count()
            })
            .OrderByDescending(v => v.ReservationCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Await all tasks
        await Task.WhenAll(
            venueStatsTask,
            newVenuesThisMonthTask,
            userStatsTask,
            newUsersThisMonthTask,
            reservationStatsTask,
            reservationsThisMonthTask,
            topVenuesTask);

        // ── Aggregate results ─────────────────────────────────────────────────
        var venueStats = await venueStatsTask;
        var userStats = await userStatsTask;
        var resStats = await reservationStatsTask;

        var approvedVenues = venueStats
            .Where(v => v.IsApproved && !v.IsDeleted).Sum(v => v.Count);
        var pendingVenues = venueStats
            .Where(v => !v.IsApproved && !v.IsDeleted).Sum(v => v.Count);
        var totalVenues = venueStats
            .Where(v => !v.IsDeleted).Sum(v => v.Count);

        var customerCount = userStats
            .FirstOrDefault(u => u.Role == UserRole.Customer)?.Count ?? 0;
        var ownerCount = userStats
            .FirstOrDefault(u => u.Role == UserRole.RestaurantOwner)?.Count ?? 0;
        var totalUsers = userStats.Sum(u => u.Count);

        int GetResCount(ReservationStatus status) =>
            resStats.FirstOrDefault(r => r.Status == status)?.Count ?? 0;

        return new AdminStatsDto
        {
            TotalVenues = totalVenues,
            ApprovedVenues = approvedVenues,
            PendingApprovalVenues = pendingVenues,
            NewVenuesThisMonth = await newVenuesThisMonthTask,
            TotalUsers = totalUsers,
            CustomerCount = customerCount,
            OwnerCount = ownerCount,
            NewUsersThisMonth = await newUsersThisMonthTask,
            TotalReservationsAllTime = resStats.Sum(r => r.Count),
            TotalReservationsThisMonth = await reservationsThisMonthTask,
            PendingReservations = GetResCount(ReservationStatus.Pending),
            ConfirmedReservations = GetResCount(ReservationStatus.Confirmed),
            TopVenuesByReservations = await topVenuesTask
        };
    }
}