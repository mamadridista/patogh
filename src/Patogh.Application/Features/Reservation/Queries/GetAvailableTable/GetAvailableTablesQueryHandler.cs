using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;
using static System.TimeSpan;

namespace Patogh.Application.Features.Reservations.Queries.GetAvailableTables;

public class GetAvailableTablesQueryHandler
    : IRequestHandler<GetAvailableTablesQuery, List<AvailableTableDto>>
{
    private static readonly TimeSpan ReservationDuration = FromHours(2);

    private readonly IApplicationDbContext _context;

    public GetAvailableTablesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableTableDto>> Handle(
        GetAvailableTablesQuery request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _context.Restaurants
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        var requestedEnd = request.StartTime.Add(ReservationDuration);

        // Get IDs of tables that are already booked (have an overlapping active reservation)
        var bookedTableIds = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.RestaurantId == request.RestaurantId &&
                r.ReservationDate == request.Date &&
                r.Status != ReservationStatus.Cancelled &&
                r.Status != ReservationStatus.Rejected &&
                r.StartTime < requestedEnd &&
                r.EndTime > request.StartTime)
            .Select(r => r.TableId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Return tables that: (a) belong to this restaurant, (b) fit the guests, (c) are not booked
        return await _context.RestaurantTables
            .AsNoTracking()
            .Where(t =>
                t.RestaurantId == request.RestaurantId &&
                t.Capacity >= request.GuestCount &&
                !bookedTableIds.Contains(t.Id))
            .OrderBy(t => t.TableNumber)
            .Select(t => new AvailableTableDto
            {
                TableId = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity
            })
            .ToListAsync(cancellationToken);
    }
}