using MediatR;
using Patogh.Application.Features.Reservations.DTOs;

namespace Patogh.Application.Features.Reservations.Commands.ConfirmReservation;

public class ConfirmReservationCommand : IRequest<ReservationResponseDto>
{
    public Guid ReservationId { get; set; }
}