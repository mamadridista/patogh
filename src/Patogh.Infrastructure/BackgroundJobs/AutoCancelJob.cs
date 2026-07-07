using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Patogh.Application.Common.Helpers;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;

namespace Patogh.Infrastructure.BackgroundJobs;

/// <summary>
/// Automatically cancels Pending reservations whose date has passed.
///
/// WHY: If an owner never responds to a reservation request and the
/// date passes, the customer is left with a "Pending" reservation
/// that is functionally dead. This job cleans those up and frees the
/// time slot for reporting accuracy.
///
/// Runs every hour via Hangfire recurring job.
/// </summary>
public class AutoCancelJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoCancelJob> _logger;

    public AutoCancelJob(
        IServiceScopeFactory scopeFactory,
        ILogger<AutoCancelJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<IApplicationDbContext>();

        var today = DateTimeHelper.TodayInTehran;

        // Find all Pending reservations whose date has passed
        var stalePending = await context.Reservations
            .Where(r =>
                r.Status == ReservationStatus.Pending &&
                r.ReservationDate < today)
            .ToListAsync();

        if (stalePending.Count == 0)
        {
            _logger.LogDebug("AutoCancelJob: no stale pending reservations found");
            return;
        }

        _logger.LogInformation(
            "AutoCancelJob: auto-cancelling {Count} stale pending reservations",
            stalePending.Count);

        var now = DateTime.UtcNow;

        foreach (var reservation in stalePending)
        {
            reservation.Status = ReservationStatus.Cancelled;
            reservation.CancelledAt = now;
            reservation.CancelledBy = "System";
            reservation.UpdatedAt = now;
        }

        await context.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation(
            "AutoCancelJob: successfully cancelled {Count} reservations",
            stalePending.Count);
    }
}