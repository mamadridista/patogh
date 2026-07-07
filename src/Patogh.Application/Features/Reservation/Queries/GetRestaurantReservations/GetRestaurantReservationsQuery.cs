using MediatR;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Reservations.DTOs;
using Patogh.Domain.Enums;

namespace Patogh.Application.Features.Reservations.Queries.GetRestaurantReservations;

public class GetRestaurantReservationsQuery
    : PagedQuery, IRequest<PaginatedResult<ReservationSummaryDto>>
{
    public Guid RestaurantId { get; set; }
    public DateOnly? Date { get; set; }
    public ReservationStatus? Status { get; set; }
}