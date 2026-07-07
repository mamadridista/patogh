using Patogh.Domain.Common;
using Patogh.Domain.Enums;

namespace Patogh.Domain.Entities;

public class Reservation : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid TableId { get; set; }

    // Who is actually coming (may differ from account holder)
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public int GuestCount { get; set; }

    public DateOnly ReservationDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; } // Always StartTime + 2 hours (business rule)

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public DateTime? CancelledAt { get; set; }
    public string? CancelledBy { get; set; } // "Customer" | "Owner" | "System"

    public bool ReminderSent { get; set; } = false;

    // Navigation Properties
    public User Customer { get; set; } = null!;
    public Restaurant Restaurant { get; set; } = null!;
    public RestaurantTable Table { get; set; } = null!;
}