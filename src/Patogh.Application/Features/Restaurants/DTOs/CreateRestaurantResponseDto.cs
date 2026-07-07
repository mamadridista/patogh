namespace Patogh.Application.Features.Restaurants.DTOs;

public class CreateRestaurantResponseDto
{
    public bool Success { get; set; }
    public Guid RestaurantId { get; set; }
    public string Message { get; set; } = string.Empty;
}