using Patogh.Domain.Common;

namespace Patogh.Domain.Entities;

public class RestaurantTable : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }

    // Navigation
    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();
}