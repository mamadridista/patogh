using MediatR;
using Patogh.Application.Features.Restaurants.DTOs;

namespace Patogh.Application.Features.Restaurants.Queries.GetOwnerStats;

public class GetOwnerStatsQuery : IRequest<OwnerStatsDto>
{
    public Guid RestaurantId { get; set; }

    // Date range for the report — defaults to last 30 days if not provided
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}