using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Helpers;
using Patogh.Application.Features.Tables.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Tables.Commands.DeleteTable;

public class DeleteTableCommandHandler
    : IRequestHandler<DeleteTableCommand, TableResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteTableCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TableResponseDto> Handle(
        DeleteTableCommand request,
        CancellationToken cancellationToken)
    {
        var table = await _context.RestaurantTables
            .Include(x => x.Restaurant)
            .FirstOrDefaultAsync(x => x.Id == request.TableId, cancellationToken);

        if (table is null)
            throw new NotFoundException("Table", request.TableId);

        if (table.Restaurant.OwnerId != _currentUser.UserId)
            throw new ForbiddenException();

        // Guard: cannot delete a table that has upcoming confirmed reservations
        var hasActiveReservations = await _context.Reservations
            .AnyAsync(r =>
                r.TableId == request.TableId &&
                (r.Status == Domain.Enums.ReservationStatus.Pending ||
                 r.Status == Domain.Enums.ReservationStatus.Confirmed) &&
                r.ReservationDate >= DateTimeHelper.TodayInTehran,
            cancellationToken);

        if (hasActiveReservations)
            throw new DomainValidationException(
                "این میز دارای رزرو فعال است و نمی‌توان آن را حذف کرد.");

        // SOFT DELETE
        table.IsDeleted = true;
        table.DeletedAt = DateTime.UtcNow;
        table.DeletedBy = _currentUser.UserId.ToString();
        table.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new TableResponseDto
        {
            Success = true,
            TableId = request.TableId,
            Message = "میز با موفقیت حذف شد."
        };
    }
}