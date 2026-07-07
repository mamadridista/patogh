using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Restaurants.DTOs;
using Patogh.Application.Interfaces;

namespace Patogh.Application.Features.Restaurants.Queries.GetAllRestaurantsForAdmin;

public class GetAllRestaurantsForAdminQueryHandler
    : IRequestHandler<GetAllRestaurantsForAdminQuery, PaginatedResult<RestaurantListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllRestaurantsForAdminQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<RestaurantListItemDto>> Handle(
        GetAllRestaurantsForAdminQuery request,
        CancellationToken cancellationToken)
    {
        // Admin sees ALL restaurants, including soft-deleted ones
        var query = _context.Restaurants
            .AsNoTracking()
            .IgnoreQueryFilters();

        if (request.IsApproved.HasValue)
            query = query.Where(r => r.IsApproved == request.IsApproved.Value);

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(r => r.Name.Contains(request.Name));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .Select(r => new RestaurantListItemDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location,
                FoodType = r.FoodType,
                PriceRange = r.PriceRange,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                IsApproved = r.IsApproved
            })
            .ToListAsync(cancellationToken);

        return PaginatedResult<RestaurantListItemDto>.Create(
            items, totalCount, request.Page, request.PageSize);
    }
}