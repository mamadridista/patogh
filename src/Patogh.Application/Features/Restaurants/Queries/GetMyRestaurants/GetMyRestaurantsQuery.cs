using MediatR;
using Patogh.Application.Features.Restaurants.DTOs;

namespace Patogh.Application.Features.Restaurants.Queries.GetMyRestaurants;

public class GetMyRestaurantsQuery : IRequest<List<MyRestaurantDto>> { }