using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Tables.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Tables.Queries.GetTablesByRestaurant;

public class GetTablesByRestaurantQueryHandler
    : IRequestHandler<GetTablesByRestaurantQuery, List<TableListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTablesByRestaurantQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TableListDto>> Handle(
        GetTablesByRestaurantQuery request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _context.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        // Table list is owner-only — customers use available-tables endpoint
        if (restaurant.OwnerId != _currentUser.UserId)
            throw new ForbiddenException();

        return await _context.RestaurantTables
            .AsNoTracking()
            .Where(t => t.RestaurantId == request.RestaurantId)
            .OrderBy(t => t.TableNumber)
            .Select(t => new TableListDto
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity
            })
            .ToListAsync(cancellationToken);
    }
}