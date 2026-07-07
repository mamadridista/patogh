namespace Patogh.Infrastructure.Configurations;

/// <summary>
/// Strongly-typed reservation configuration. Eliminates magic numbers in
/// reservation business logic. Bound from appsettings.json "ReservationSettings".
/// </summary>
public class ReservationSettings
{
    /// <summary>Default reservation slot duration in hours. Default: 2.</summary>
    public double DefaultDurationHours { get; set; } = 2.0;

    /// <summary>
    /// How many minutes before a reservation to send a reminder SMS. Default: 60.
    /// </summary>
    public int ReminderBeforeMinutes { get; set; } = 60;

    /// <summary>
    /// How many hours after a Pending reservation's start time to auto-cancel. Default: 1.
    /// </summary>
    public int AutoCancelAfterHours { get; set; } = 1;
}
