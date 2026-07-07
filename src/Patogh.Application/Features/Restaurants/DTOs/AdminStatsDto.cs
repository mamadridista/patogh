namespace Patogh.Application.Features.Restaurants.DTOs;

public class AdminStatsDto
{
    // Platform venue stats
    public int TotalVenues { get; set; }
    public int ApprovedVenues { get; set; }
    public int PendingApprovalVenues { get; set; }
    public int RejectedVenues { get; set; }
    public int NewVenuesThisMonth { get; set; }

    // User stats
    public int TotalUsers { get; set; }
    public int CustomerCount { get; set; }
    public int OwnerCount { get; set; }
    public int NewUsersThisMonth { get; set; }

    // Reservation stats
    public int TotalReservationsAllTime { get; set; }
    public int TotalReservationsThisMonth { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }

    // Top performers
    public List<TopVenueDto> TopVenuesByReservations { get; set; } = [];
}

public class TopVenueDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int ReservationCount { get; set; }
}