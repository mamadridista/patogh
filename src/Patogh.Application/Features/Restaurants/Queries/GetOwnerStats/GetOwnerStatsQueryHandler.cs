using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Helpers;
using Patogh.Application.Features.Restaurants.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Restaurants.Queries.GetOwnerStats;

public class GetOwnerStatsQueryHandler
    : IRequestHandler<GetOwnerStatsQuery, OwnerStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetOwnerStatsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<OwnerStatsDto> Handle(
        GetOwnerStatsQuery request,
        CancellationToken cancellationToken)
    {
        // Verify ownership
        var restaurant = await _context.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.Id == request.RestaurantId,
                cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        if (restaurant.OwnerId != _currentUser.UserId)
            throw new ForbiddenException();

        // Default to last 30 days if no range provided
        var today = DateTimeHelper.TodayInTehran;
        var from = request.From ?? today.AddDays(-30);
        var to = request.To ?? today;

        // Load all reservations in range as a single query — project to a
        // lightweight struct for all in-memory calculations below.
        // This avoids multiple round-trips and N+1 problems.
        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.RestaurantId == request.RestaurantId &&
                r.ReservationDate >= from &&
                r.ReservationDate <= to)
            .Select(r => new
            {
                r.Status,
                r.GuestCount,
                r.ReservationDate,
                r.StartTime,
                r.TableId,
                TableNumber = r.Table.TableNumber,
                TableCapacity = r.Table.Capacity
            })
            .ToListAsync(cancellationToken);

        if (reservations.Count == 0)
        {
            return new OwnerStatsDto
            {
                ReservationsByDate = [],
                TopTables = []
            };
        }

        // Status counts
        var total = reservations.Count;
        var pending = reservations.Count(r => r.Status == ReservationStatus.Pending);
        var confirmed = reservations.Count(r => r.Status == ReservationStatus.Confirmed);
        var rejected = reservations.Count(r => r.Status == ReservationStatus.Rejected);
        var cancelled = reservations.Count(r => r.Status == ReservationStatus.Cancelled);

        // Guest metrics (only count confirmed bookings for meaningful data)
        var confirmedRes = reservations.Where(r => r.Status == ReservationStatus.Confirmed).ToList();
        var totalGuests = confirmedRes.Sum(r => r.GuestCount);
        var avgGuests = confirmedRes.Count > 0
            ? Math.Round((double)totalGuests / confirmedRes.Count, 1)
            : 0;

        // Busiest day of week (among confirmed reservations)
        var busiestDay = confirmedRes
            .GroupBy(r => r.ReservationDate.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        var persianDayNames = new Dictionary<DayOfWeek, string>
        {
            [DayOfWeek.Saturday] = "شنبه",
            [DayOfWeek.Sunday] = "یکشنبه",
            [DayOfWeek.Monday] = "دوشنبه",
            [DayOfWeek.Tuesday] = "سه‌شنبه",
            [DayOfWeek.Wednesday] = "چهارشنبه",
            [DayOfWeek.Thursday] = "پنجشنبه",
            [DayOfWeek.Friday] = "جمعه"
        };

        var busiestDayName = persianDayNames.GetValueOrDefault(busiestDay, "-");

        // Busiest hour (hour of StartTime among confirmed reservations)
        var busiestHour = confirmedRes
            .GroupBy(r => r.StartTime.Hours)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        // Occupancy rate: confirmed reservations / total reservations in range
        var occupancyRate = total > 0
            ? Math.Round((double)confirmed / total * 100, 1)
            : 0;

        // Daily chart data — include all days in range (even zero-count days)
        var dayCount = (to.DayNumber - from.DayNumber) + 1;
        var reservationsByDate = Enumerable.Range(0, dayCount)
            .Select(i => from.AddDays(i))
            .Select(date => new DailyReservationCountDto
            {
                Date = date,
                Count = reservations.Count(r => r.ReservationDate == date)
            })
            .ToList();

        // Top tables by reservation count
        var topTables = reservations
            .GroupBy(r => new { r.TableId, r.TableNumber, r.TableCapacity })
            .Select(g => new TableReservationCountDto
            {
                TableNumber = g.Key.TableNumber,
                Capacity = g.Key.TableCapacity,
                ReservationCount = g.Count()
            })
            .OrderByDescending(t => t.ReservationCount)
            .Take(5)
            .ToList();

        return new OwnerStatsDto
        {
            TotalReservations = total,
            PendingCount = pending,
            ConfirmedCount = confirmed,
            RejectedCount = rejected,
            CancelledCount = cancelled,
            AverageGuestCount = avgGuests,
            TotalGuests = totalGuests,
            BusiestDayOfWeek = busiestDayName,
            BusiestHour = busiestHour,
            OccupancyRate = occupancyRate,
            ReservationsByDate = reservationsByDate,
            TopTables = topTables
        };
    }
}