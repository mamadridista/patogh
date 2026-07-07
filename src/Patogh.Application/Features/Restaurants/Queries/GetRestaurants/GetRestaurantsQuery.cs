using MediatR;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Restaurants.DTOs;

namespace Patogh.Application.Features.Restaurants.Queries.GetRestaurants;

public class GetRestaurantsQuery
    : PagedQuery, IRequest<PaginatedResult<RestaurantListItemDto>>
{
    public string? Name { get; set; }
    public string? FoodType { get; set; }
    public string? Location { get; set; }
    public string? PriceRange { get; set; }
}