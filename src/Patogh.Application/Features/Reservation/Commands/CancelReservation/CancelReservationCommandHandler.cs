using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Reservations.Commands.CancelReservation;

public class CancelReservationCommandHandler
    : IRequestHandler<CancelReservationCommand, ReservationResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISmsSender _smsSender;

    public CancelReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISmsSender smsSender)
    {
        _context = context;
        _currentUser = currentUser;
        _smsSender = smsSender;
    }

    public async Task<ReservationResponseDto> Handle(
        CancelReservationCommand request,
        CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Restaurant)
                .ThenInclude(r => r.Owner)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Reservation", request.ReservationId);

        var isCustomer = reservation.CustomerId == _currentUser.UserId;
        var isOwner = reservation.Restaurant.OwnerId == _currentUser.UserId;

        // Only the customer who made the reservation or the restaurant owner can cancel it
        if (!isCustomer && !isOwner)
            throw new UnauthorizedDomainException();

        if (reservation.Status == ReservationStatus.Cancelled)
            throw new DomainValidationException("این رزرو قبلاً لغو شده است.");

        if (reservation.Status == ReservationStatus.Rejected)
            throw new DomainValidationException("رزرو رد شده را نمی‌توان لغو کرد.");

        // Business rule: customer cannot cancel a past reservation.
        // This prevents customers from gaming the system by cancelling
        // reservations they didn't show up to, after the fact.
        if (isCustomer &&
            reservation.ReservationDate < DateOnly.FromDateTime(DateTime.Today))
        {
            throw new DomainValidationException(
                "رزروهای گذشته را نمی‌توان لغو کرد.");
        }

        var cancelledBy = isCustomer ? "Customer" : "Owner";

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = DateTime.UtcNow;
        reservation.CancelledBy = cancelledBy;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Cross-notify: customer cancels → notify owner; owner cancels → notify customer
        if (isCustomer)
        {
            _ = _smsSender.SendAsync(
                reservation.Restaurant.Owner.PhoneNumber,
                $"رزرو مشتری {reservation.CustomerName} برای تاریخ " +
                $"{reservation.ReservationDate} ساعت {reservation.StartTime:hh\\:mm} " +
                $"لغو شد.");
        }
        else
        {
            _ = _smsSender.SendAsync(
                reservation.CustomerPhone,
                $"رزرو شما در {reservation.Restaurant.Name} برای تاریخ " +
                $"{reservation.ReservationDate} توسط رستوران لغو شد.");
        }

        return new ReservationResponseDto
        {
            Success = true,
            ReservationId = reservation.Id,
            Message = "رزرو با موفقیت لغو شد."
        };
    }
}