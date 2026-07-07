namespace Patogh.Application.Features.Restaurants.DTOs;

public class RestaurantDetailsDto
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

    // Image URLs resolved to full paths
    public string? CoverImageUrl { get; set; }
    public List<string> GalleryImageUrls { get; set; } = [];

    public List<MenuItemDto> MenuItems { get; set; } = [];
    public List<RestaurantTableDto> Tables { get; set; } = [];
}