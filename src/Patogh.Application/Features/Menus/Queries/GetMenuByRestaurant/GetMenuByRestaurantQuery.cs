using MediatR;
using Patogh.Application.Features.Menus.DTOs;

namespace Patogh.Application.Features.Menus.Queries.GetMenuByRestaurant;

public class GetMenuByRestaurantQuery : IRequest<List<MenuItemListDto>>
{
    public Guid RestaurantId { get; set; }
}