using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Tables.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Tables.Commands.CreateTable;

public class CreateTableCommandHandler
    : IRequestHandler<CreateTableCommand, TableResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTableCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TableResponseDto> Handle(
        CreateTableCommand request,
        CancellationToken cancellationToken)
    {
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);

        if (restaurant is null)
            throw new NotFoundException("Restaurant", request.RestaurantId);

        if (restaurant.OwnerId != _currentUser.UserId)
            throw new UnauthorizedDomainException();

        var exists = await _context.RestaurantTables
            .AnyAsync(x =>
                x.RestaurantId == request.RestaurantId &&
                x.TableNumber == request.TableNumber,
                cancellationToken);

        if (exists)
            throw new ConflictException($"میز شماره {request.TableNumber} در این رستوران وجود دارد.");

        var table = new RestaurantTable
        {
            RestaurantId = request.RestaurantId,
            TableNumber = request.TableNumber,
            Capacity = request.Capacity
        };

        await _context.RestaurantTables.AddAsync(table, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new TableResponseDto
        {
            Success = true,
            TableId = table.Id,
            Message = "میز با موفقیت تعریف شد."
        };
    }
}