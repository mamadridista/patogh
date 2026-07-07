using MediatR;
using Patogh.Application.Features.Reservations.DTOs;

namespace Patogh.Application.Features.Reservations.Commands.CreateReservation;

// Must be a record to support `with` expressions in tests.
public record CreateReservationCommand : IRequest<ReservationResponseDto>
{
    public Guid RestaurantId { get; init; }
    public Guid TableId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public int GuestCount { get; init; }
    public DateOnly ReservationDate { get; init; }
    public TimeSpan StartTime { get; init; }
    public string? Notes { get; init; }
}