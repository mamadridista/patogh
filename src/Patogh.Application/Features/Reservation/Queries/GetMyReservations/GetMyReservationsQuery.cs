using MediatR;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Domain.Enums;

namespace Patogh.Application.Features.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQuery
    : PagedQuery, IRequest<PaginatedResult<ReservationSummaryDto>>
{
    public ReservationStatus? Status { get; set; }
}