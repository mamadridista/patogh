using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Restaurants.DTOs;
using Patogh.Application.Interfaces;

namespace Patogh.Application.Features.Restaurants.Queries.GetRestaurants;

public class GetRestaurantsQueryHandler
    : IRequestHandler<GetRestaurantsQuery, PaginatedResult<RestaurantListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public GetRestaurantsQueryHandler(
        IApplicationDbContext context,
        ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<PaginatedResult<RestaurantListItemDto>> Handle(
        GetRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        // Only cache the default first page with no filters.
        // Any filtered or paged request hits the database directly.
        var isDefaultQuery = string.IsNullOrWhiteSpace(request.Name)
                          && string.IsNullOrWhiteSpace(request.FoodType)
                          && string.IsNullOrWhiteSpace(request.Location)
                          && string.IsNullOrWhiteSpace(request.PriceRange)
                          && request.Page == 1
                          && request.PageSize == 20;

        if (isDefaultQuery)
        {
            const string cacheKey = "venues:page1:default";
            var cached = await _cache.GetAsync<PaginatedResult<RestaurantListItemDto>>(cacheKey);
            if (cached is not null)
                return cached;
        }

        var query = _context.Restaurants
            .Where(x => x.IsApproved)
            .AsNoTracking();

        // Filtering
        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(x => x.Name.Contains(request.Name));

        if (!string.IsNullOrWhiteSpace(request.FoodType))
            query = query.Where(x => x.FoodType == request.FoodType);

        if (!string.IsNullOrWhiteSpace(request.Location))
            query = query.Where(x => x.Location.Contains(request.Location));

        if (!string.IsNullOrWhiteSpace(request.PriceRange))
            query = query.Where(x => x.PriceRange == request.PriceRange);

        // Count before pagination (needed for TotalPages calculation)
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .Select(x => new RestaurantListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Location = x.Location,
                FoodType = x.FoodType,
                PriceRange = x.PriceRange,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                IsApproved = x.IsApproved
            })
            .ToListAsync(cancellationToken);

        var result = PaginatedResult<RestaurantListItemDto>.Create(
            items, totalCount, request.Page, request.PageSize);

        if (isDefaultQuery)
            await _cache.SetAsync("venues:page1:default", result, TimeSpan.FromMinutes(5));

        return result;
    }
}