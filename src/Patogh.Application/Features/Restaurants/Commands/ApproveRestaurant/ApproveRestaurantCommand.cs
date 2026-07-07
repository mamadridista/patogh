using MediatR;
using Patogh.Application.Features.Restaurants.DTOs;

namespace Patogh.Application.Features.Restaurants.Commands.ApproveRestaurant;

public class ApproveRestaurantCommand : IRequest<ApproveRestaurantResponseDto>
{
    public Guid RestaurantId { get; set; }
    public bool IsApproved { get; set; }
}