using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Reservations.Queries.GetReservationById;

public class GetReservationByIdQueryHandler
    : IRequestHandler<GetReservationByIdQuery, ReservationSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetReservationByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ReservationSummaryDto> Handle(
        GetReservationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Id == request.ReservationId)
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
                CreatedAt = r.CreatedAt,
                OwnerId = r.Restaurant.OwnerId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Reservation", request.ReservationId);

        // Only the customer who made it OR the restaurant owner can view it
        var isCustomer = reservation.CustomerId == _currentUser.UserId;
        var isOwner = reservation.OwnerId == _currentUser.UserId;

        if (!isCustomer && !isOwner)
            throw new ForbiddenException();

        return reservation;
    }
}