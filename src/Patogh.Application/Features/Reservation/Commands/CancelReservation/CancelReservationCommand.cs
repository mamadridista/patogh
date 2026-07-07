using MediatR;
using Patogh.Application.Features.Reservations.DTOs;

namespace Patogh.Application.Features.Reservations.Commands.CancelReservation;

public class CancelReservationCommand : IRequest<ReservationResponseDto>
{
    public Guid ReservationId { get; set; }
}