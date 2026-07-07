using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Restaurants.DTOs;
using Patogh.Application.Interfaces;

namespace Patogh.Application.Features.Restaurants.Queries.GetMyRestaurants;

public class GetMyRestaurantsQueryHandler
    : IRequestHandler<GetMyRestaurantsQuery, List<MyRestaurantDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyRestaurantsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<MyRestaurantDto>> Handle(
        GetMyRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Restaurants
            .AsNoTracking()
            .Where(r => r.OwnerId == _currentUser.UserId)
            .Select(r => new MyRestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location,
                FoodType = r.FoodType,
                PriceRange = r.PriceRange,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                IsApproved = r.IsApproved,
                TableCount = r.Tables.Count(t => !t.IsDeleted),
                MenuItemCount = r.MenuItems.Count(m => !m.IsDeleted),
                CreatedAt = r.CreatedAt
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}