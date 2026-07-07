using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Admin.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandler
    : IRequestHandler<ChangeUserRoleCommand, ChangeUserRoleResponseDto>
{
    private readonly IApplicationDbContext _context;

    public ChangeUserRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChangeUserRoleResponseDto> Handle(
        ChangeUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException("User", request.UserId);

        // Prevent demoting or promoting Admin users — admin accounts
        // should only be created via direct DB seed, not via API
        if (user.Role == UserRole.Admin)
            throw new ForbiddenException(
                "نقش کاربران ادمین قابل تغییر از طریق API نیست.");

        if (request.NewRole == UserRole.Admin)
            throw new ForbiddenException(
                "ارتقاء کاربر به ادمین از طریق API مجاز نیست.");

        user.Role = request.NewRole;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ChangeUserRoleResponseDto
        {
            Success = true,
            Message = $"نقش کاربر با موفقیت به {request.NewRole} تغییر یافت."
        };
    }
}