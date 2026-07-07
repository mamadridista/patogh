using MediatR;
using Patogh.Application.Features.Tables.DTOs;

namespace Patogh.Application.Features.Tables.Queries.GetTablesByRestaurant;

public class GetTablesByRestaurantQuery : IRequest<List<TableListDto>>
{
    public Guid RestaurantId { get; set; }
}