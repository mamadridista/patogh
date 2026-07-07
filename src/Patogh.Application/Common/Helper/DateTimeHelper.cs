using static System.Net.Mime.MediaTypeNames;

namespace Patogh.Application.Common.Helpers;

/// <summary>
/// Centralizes all timezone-aware DateTime operations for the Patogh platform.
///
/// WHY THIS EXISTS:
/// Iran Standard Time (IRST) is UTC+3:30 — unusual because of the 30-minute offset.
/// Using DateTime.UtcNow directly and comparing it against user-facing dates/times
/// produces wrong results: a 23:00 Tehran reservation would appear "in the past"
/// if compared against 23:30 UTC.
///
/// All business logic that involves "now", "today", or "current time"
/// MUST use this helper instead of DateTime.UtcNow or DateTime.Now.
/// </summary>
public static class DateTimeHelper
{
    private static readonly TimeZoneInfo TehranZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            // Linux/macOS identifier:
            OperatingSystem.IsWindows()
                ? "Iran Standard Time"
                : "Asia/Tehran");

    /// <summary>Current date and time in Tehran timezone.</summary>
    public static DateTime NowInTehran =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TehranZone);

    /// <summary>Today's date in Tehran timezone.</summary>
    public static DateOnly TodayInTehran =>
        DateOnly.FromDateTime(NowInTehran);

    /// <summary>Current time-of-day in Tehran timezone.</summary>
    public static TimeSpan CurrentTimeInTehran =>
        NowInTehran.TimeOfDay;

    /// <summary>
    /// Converts a UTC DateTime to Tehran local time.
    /// Use when displaying stored UTC timestamps to the user.
    /// </summary>
    public static DateTime ToTehranTime(DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Local)
            utcDateTime = utcDateTime.ToUniversalTime();

        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
            TehranZone);
    }

    /// <summary>
    /// Converts a Tehran local DateTime to UTC for storage.
    /// Use before saving user-provided dates to the database.
    /// </summary>
    public static DateTime ToUtc(DateTime tehranDateTime) =>
        TimeZoneInfo.ConvertTimeToUtc(tehranDateTime, TehranZone);

    /// <summary>
    /// Returns true if the given reservation date+time is in the past
    /// relative to current Tehran time.
    /// </summary>
    public static bool IsInPast(DateOnly reservationDate, TimeSpan reservationTime)
    {
        var now = NowInTehran;
        var today = DateOnly.FromDateTime(now);

        if (reservationDate < today)
            return true;

        if (reservationDate == today && reservationTime < now.TimeOfDay)
            return true;

        return false;
    }

    /// <summary>
    /// Returns true if the reservation starts within the next [minutes] minutes.
    /// Used by the reminder background job.
    /// </summary>
    public static bool IsWithinMinutes(
        DateOnly reservationDate,
        TimeSpan reservationTime,
        int minutes)
    {
        var now = NowInTehran;
        var reservationDateTime = reservationDate.ToDateTime(TimeOnly.FromTimeSpan(reservationTime));
        var diff = reservationDateTime - now;
        return diff > TimeSpan.Zero && diff <= TimeSpan.FromMinutes(minutes);
    }
}