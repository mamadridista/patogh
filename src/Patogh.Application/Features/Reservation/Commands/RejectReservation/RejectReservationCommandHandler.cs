using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Reservations.Commands.RejectReservation;

public class RejectReservationCommandHandler
    : IRequestHandler<RejectReservationCommand, ReservationResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISmsSender _smsSender;

    public RejectReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISmsSender smsSender)
    {
        _context = context;
        _currentUser = currentUser;
        _smsSender = smsSender;
    }

    public async Task<ReservationResponseDto> Handle(
        RejectReservationCommand request,
        CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Reservation", request.ReservationId);

        if (reservation.Restaurant.OwnerId != _currentUser.UserId)
            throw new UnauthorizedDomainException();

        if (reservation.Status != ReservationStatus.Pending)
            throw new DomainValidationException(
                "فقط رزروهای در وضعیت 'در انتظار' را می‌توان رد کرد.");

        reservation.Status = ReservationStatus.Rejected;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Notify customer that the reservation was rejected
        _ = _smsSender.SendAsync(
            reservation.CustomerPhone,
            $"متأسفانه رزرو شما در {reservation.Restaurant.Name} برای تاریخ " +
            $"{reservation.ReservationDate} رد شد. لطفاً زمان دیگری انتخاب کنید.");

        return new ReservationResponseDto
        {
            Success = true,
            ReservationId = reservation.Id,
            Message = "رزرو رد شد."
        };
    }
}