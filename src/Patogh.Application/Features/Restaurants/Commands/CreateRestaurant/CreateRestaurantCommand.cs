using MediatR;
using Patogh.Application.Features.Restaurants.DTOs;

namespace Patogh.Application.Features.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommand : IRequest<CreateRestaurantResponseDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string FoodType { get; set; } = string.Empty;
    public string PriceRange { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public List<Guid> Images { get; set; } = new();
}