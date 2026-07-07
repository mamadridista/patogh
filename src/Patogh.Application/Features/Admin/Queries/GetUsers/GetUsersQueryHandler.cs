using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Admin.DTOs;
using Patogh.Application.Interfaces;

namespace Patogh.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, PaginatedResult<AdminUserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<AdminUserDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            query = query.Where(u => u.PhoneNumber.Contains(request.PhoneNumber));

        if (!string.IsNullOrWhiteSpace(request.Role) &&
            Enum.TryParse<Domain.Enums.UserRole>(request.Role, out var role))
            query = query.Where(u => u.Role == role);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip(request.Skip)
            .Take(request.PageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt,
                ReservationCount = _context.Reservations.Count(r => r.CustomerId == u.Id),
                RestaurantCount = _context.Restaurants.Count(r => r.OwnerId == u.Id)
            })
            .ToListAsync(cancellationToken);

        return PaginatedResult<AdminUserDto>.Create(
            items, totalCount, request.Page, request.PageSize);
    }
}