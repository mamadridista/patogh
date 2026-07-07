using MediatR;
using Patogh.Application.Features.Menus.DTOs;

namespace Patogh.Application.Features.Menus.Commands.UpdateMenuItem;

public class UpdateMenuItemCommand : IRequest<MenuItemResponseDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid? ImageId { get; set; }
}