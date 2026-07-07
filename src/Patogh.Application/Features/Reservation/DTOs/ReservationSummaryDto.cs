using Patogh.Domain.Enums;

namespace Patogh.Application.Features.Reservations.DTOs;

public class ReservationSummaryDto
{
    public Guid Id { get; set; }

    // Restaurant info
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;

    // Table info
    public int TableNumber { get; set; }
    public int TableCapacity { get; set; }

    // Booking info
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public DateOnly ReservationDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Notes { get; set; }

    // Status
    public ReservationStatus Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Used internally for authorization — not serialized to client
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid CustomerId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Guid OwnerId { get; set; }
}