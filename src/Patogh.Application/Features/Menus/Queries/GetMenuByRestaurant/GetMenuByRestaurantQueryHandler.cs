using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Menus.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Menus.Queries.GetMenuByRestaurant;

public class GetMenuByRestaurantQueryHandler
    : IRequestHandler<GetMenuByRestaurantQuery, List<MenuItemListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMenuByRestaurantQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MenuItemListDto>> Handle(
        GetMenuByRestaurantQuery request,
        CancellationToken cancellationToken)
    {
        // Verify restaurant exists and is approved (public endpoint)
        var restaurantExists = await _context.Restaurants
            .AsNoTracking()
            .AnyAsync(r => r.Id == request.RestaurantId && r.IsApproved,
                cancellationToken);

        if (!restaurantExists)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        return await _context.MenuItems
            .AsNoTracking()
            .Where(m => m.RestaurantId == request.RestaurantId)
            .OrderBy(m => m.Name)
            .Select(m => new MenuItemListDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                // Return the stored file path — frontend prepends the base URL
                ImageUrl = m.Image != null ? m.Image.FilePath : null
            })
            .ToListAsync(cancellationToken);
    }
}