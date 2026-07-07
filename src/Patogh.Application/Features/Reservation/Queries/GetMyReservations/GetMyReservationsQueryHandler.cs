using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Application.Interfaces;

namespace Patogh.Application.Features.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQueryHandler
    : IRequestHandler<GetMyReservationsQuery, PaginatedResult<ReservationSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyReservationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<ReservationSummaryDto>> Handle(
        GetMyReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Reservations
            .AsNoTracking()
            .Where(r => r.CustomerId == _currentUser.UserId);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.ReservationDate)
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