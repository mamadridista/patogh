using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Patogh.Application.Common.Helpers;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;

namespace Patogh.Infrastructure.BackgroundJobs;

/// <summary>
/// Sends SMS reminders to customers whose confirmed reservation
/// starts within the next 60–75 minutes (Tehran time).
///
/// Runs every 30 minutes via Hangfire recurring job.
/// The 15-minute window (60–75 min before) ensures a reservation
/// that starts in exactly 61 minutes is caught on the next run
/// even if this run runs slightly late.
/// </summary>
public class ReservationReminderJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationReminderJob> _logger;

    // Window: send reminder if reservation is between 60 and 75 minutes away
    private const int ReminderWindowMinutes = 75;
    private const int ReminderWindowFloorMinutes = 60;

    public ReservationReminderJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationReminderJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        // Hangfire jobs must create their own scope because they run
        // outside the request pipeline — no HttpContext, no scoped services
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<IApplicationDbContext>();
        var smsSender = scope.ServiceProvider
            .GetRequiredService<ISmsSender>();

        var today = DateTimeHelper.TodayInTehran;
        var nowTime = DateTimeHelper.CurrentTimeInTehran;

        // Find the time window: reservations starting in 60–75 minutes
        var windowStart = nowTime.Add(TimeSpan.FromMinutes(ReminderWindowFloorMinutes));
        var windowEnd = nowTime.Add(TimeSpan.FromMinutes(ReminderWindowMinutes));

        var reservations = await context.Reservations
            .Where(r =>
                r.Status == ReservationStatus.Confirmed &&
                r.ReminderSent == false &&
                r.ReservationDate == today &&
                r.StartTime >= windowStart &&
                r.StartTime <= windowEnd)
            .Include(r => r.Restaurant)
            .ToListAsync();

        if (reservations.Count == 0)
        {
            _logger.LogDebug("ReservationReminderJob: no reservations to remind at {Time}", nowTime);
            return;
        }

        _logger.LogInformation(
            "ReservationReminderJob: sending {Count} reminders", reservations.Count);

        foreach (var reservation in reservations)
        {
            try
            {
                await smsSender.SendAsync(
                    reservation.CustomerPhone,
                    $"یادآوری رزرو: رزرو شما در {reservation.Restaurant.Name} " +
                    $"امروز ساعت {reservation.StartTime:hh\\:mm} است. " +
                    $"میز شماره {reservation.Table?.TableNumber} برای {reservation.GuestCount} نفر آماده است.");

                reservation.ReminderSent = true;
                reservation.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // Log but don't rethrow — one SMS failure shouldn't stop other reminders
                _logger.LogError(ex,
                    "Failed to send reminder for reservation {ReservationId}",
                    reservation.Id);
            }
        }

        await context.SaveChangesAsync(CancellationToken.None);
    }
}