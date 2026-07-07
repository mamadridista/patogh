using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Menus.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Menus.Commands.CreateMenuItem;

public class CreateMenuItemCommandHandler
    : IRequestHandler<CreateMenuItemCommand, MenuItemResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateMenuItemCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MenuItemResponseDto> Handle(
        CreateMenuItemCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        if (restaurant.OwnerId != _currentUser.UserId)
            throw new UnauthorizedDomainException();

        var menuItem = new MenuItem
        {
            RestaurantId = request.RestaurantId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageId = request.ImageId
        };

        await _context.MenuItems.AddAsync(menuItem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new MenuItemResponseDto
        {
            Success = true,
            MenuItemId = menuItem.Id,
            Message = "آیتم منو با موفقیت اضافه شد."
        };
    }
}