namespace Patogh.Application.Features.Admin.DTOs;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ReservationCount { get; set; }
    public int RestaurantCount { get; set; }
}