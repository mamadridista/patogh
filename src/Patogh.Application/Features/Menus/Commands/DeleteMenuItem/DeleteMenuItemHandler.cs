using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Menus.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Menus.Commands.DeleteMenuItem;

public class DeleteMenuItemCommandHandler
    : IRequestHandler<DeleteMenuItemCommand, MenuItemResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteMenuItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MenuItemResponseDto> Handle(
        DeleteMenuItemCommand request,
        CancellationToken cancellationToken)
    {
        var menuItem = await _context.MenuItems
            .Include(x => x.Restaurant)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (menuItem is null)
            throw new NotFoundException("MenuItem", request.Id);

        if (menuItem.Restaurant.OwnerId != _currentUser.UserId)
            throw new UnauthorizedDomainException();

        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync(cancellationToken);

        return new MenuItemResponseDto
        {
            Success = true,
            MenuItemId = request.Id,
            Message = "آیتم منو با موفقیت حذف شد."
        };
    }
}