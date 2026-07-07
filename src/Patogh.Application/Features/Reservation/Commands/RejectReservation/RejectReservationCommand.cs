using MediatR;
using Patogh.Application.Features.Reservations.DTOs;

namespace Patogh.Application.Features.Reservations.Commands.RejectReservation;

public class RejectReservationCommand : IRequest<ReservationResponseDto>
{
    public Guid ReservationId { get; set; }
}