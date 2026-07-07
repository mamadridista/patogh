namespace Patogh.Application.Features.Restaurants.DTOs;

public class OwnerStatsDto
{
    // Summary counts
    public int TotalReservations { get; set; }
    public int PendingCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }

    // Guest metrics
    public double AverageGuestCount { get; set; }
    public int TotalGuests { get; set; }

    // Time-based insights
    public string BusiestDayOfWeek { get; set; } = string.Empty;
    public int BusiestHour { get; set; }

    // Occupancy: confirmed / total possible slots in period
    public double OccupancyRate { get; set; }

    // Chart data: reservations per day in the requested range
    public List<DailyReservationCountDto> ReservationsByDate { get; set; } = [];

    // Table performance
    public List<TableReservationCountDto> TopTables { get; set; } = [];
}

public class DailyReservationCountDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public class TableReservationCountDto
{
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
    public int ReservationCount { get; set; }
}