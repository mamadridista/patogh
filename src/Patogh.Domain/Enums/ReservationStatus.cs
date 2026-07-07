namespace Patogh.Domain.Enums;

public enum ReservationStatus
{
    Pending = 1,    // Created by customer, awaiting owner action
    Confirmed = 2,  // Owner accepted
    Rejected = 3,   // Owner rejected
    Cancelled = 4   // Cancelled by customer or owner after confirmation
}