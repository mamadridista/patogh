using MediatR;
using Patogh.Application.Features.Menus.DTOs;

namespace Patogh.Application.Features.Menus.Commands.DeleteMenuItem;

public class DeleteMenuItemCommand : IRequest<MenuItemResponseDto>
{
    public Guid Id { get; set; }
}