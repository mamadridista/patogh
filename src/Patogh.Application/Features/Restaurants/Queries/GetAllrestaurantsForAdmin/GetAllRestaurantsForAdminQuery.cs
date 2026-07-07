using MediatR;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Restaurants.DTOs;

namespace Patogh.Application.Features.Restaurants.Queries.GetAllRestaurantsForAdmin;

public class GetAllRestaurantsForAdminQuery
    : PagedQuery, IRequest<PaginatedResult<RestaurantListItemDto>>
{
    // Admin can filter by approval status
    public bool? IsApproved { get; set; }
    public string? Name { get; set; }
}