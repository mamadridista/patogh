using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Reservations.Commands.ConfirmReservation;

public class ConfirmReservationCommandHandler
    : IRequestHandler<ConfirmReservationCommand, ReservationResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISmsSender _smsSender;

    public ConfirmReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ISmsSender smsSender)
    {
        _context = context;
        _currentUser = currentUser;
        _smsSender = smsSender;
    }

    public async Task<ReservationResponseDto> Handle(
        ConfirmReservationCommand request,
        CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Restaurant)
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken);

        if (reservation is null)
            throw new NotFoundException("Reservation", request.ReservationId);

        // Only the owner of the restaurant associated with this reservation can confirm it
        if (reservation.Restaurant.OwnerId != _currentUser.UserId)
            throw new UnauthorizedDomainException();

        if (reservation.Status != ReservationStatus.Pending)
            throw new DomainValidationException(
                "فقط رزروهای در وضعیت 'در انتظار' را می‌توان تأیید کرد.");

        reservation.Status = ReservationStatus.Confirmed;
        reservation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Notify customer via SMS after successful save.
        // Fire-and-forget: if SMS fails, the confirmation is still committed.
        // Sprint 3 will add a retry mechanism via Hangfire.
        _ = _smsSender.SendAsync(
            reservation.CustomerPhone,
            $"رزرو شما در {reservation.Restaurant.Name} برای تاریخ " +
            $"{reservation.ReservationDate} ساعت {reservation.StartTime:hh\\:mm} " +
            $"تأیید شد.");

        return new ReservationResponseDto
        {
            Success = true,
            ReservationId = reservation.Id,
            Message = "رزرو با موفقیت تأیید شد."
        };
    }
}