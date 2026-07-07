using MediatR;
using Patogh.Application.Features.Reservations.DTOs;

namespace Patogh.Application.Features.Reservations.Queries.GetAvailableTables;

public class GetAvailableTablesQuery : IRequest<List<AvailableTableDto>>
{
    public Guid RestaurantId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public int GuestCount { get; set; }
}