namespace Patogh.Application.Features.Restaurants.DTOs;

public class MyRestaurantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string FoodType { get; set; } = string.Empty;
    public string PriceRange { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsApproved { get; set; }
    public int TableCount { get; set; }
    public int MenuItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}