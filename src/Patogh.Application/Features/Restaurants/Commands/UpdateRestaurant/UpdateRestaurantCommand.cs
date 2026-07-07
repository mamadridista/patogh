using MediatR;

namespace Patogh.Application.Features.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommand : IRequest<UpdateRestaurantResponseDto>
{
    // Set from route — not from body
    public Guid RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string FoodType { get; set; } = string.Empty;
    public string PriceRange { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid? CoverImageId { get; set; }
}

public class UpdateRestaurantResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}