using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Reservations.Queries.GetRestaurantReservations;

public class GetRestaurantReservationsQueryHandler
    : IRequestHandler<GetRestaurantReservationsQuery, PaginatedResult<ReservationSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetRestaurantReservationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ReservationSummaryDto>> Handle(
        GetRestaurantReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _context.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        if (restaurant.OwnerId != _currentUser.UserId)
            throw new ForbiddenException();

        var query = _context.Reservations
            .AsNoTracking()
            .Where(r => r.RestaurantId == request.RestaurantId);

        if (request.Date.HasValue)
            query = query.Where(r => r.ReservationDate == request.Date.Value);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(r => r.ReservationDate)
            .ThenBy(r => r.StartTime)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .Select(r => new ReservationSummaryDto
            {
                Id = r.Id,
                RestaurantId = r.RestaurantId,
                RestaurantName = r.Restaurant.Name,
                TableNumber = r.Table.TableNumber,
                TableCapacity = r.Table.Capacity,
                CustomerName = r.CustomerName,
                CustomerPhone = r.CustomerPhone,
                GuestCount = r.GuestCount,
                ReservationDate = r.ReservationDate,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Notes = r.Notes,
                Status = r.Status,
                StatusLabel = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return PaginatedResult<ReservationSummaryDto>.Create(
            items, totalCount, request.Page, request.PageSize);
    }
}