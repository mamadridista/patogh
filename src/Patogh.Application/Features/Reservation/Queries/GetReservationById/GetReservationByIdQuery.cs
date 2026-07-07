using MediatR;
using Patogh.Application.Features.Reservations.DTOs;

namespace Patogh.Application.Features.Reservations.Queries.GetReservationById;

public class GetReservationByIdQuery : IRequest<ReservationSummaryDto>
{
    public Guid ReservationId { get; set; }
}