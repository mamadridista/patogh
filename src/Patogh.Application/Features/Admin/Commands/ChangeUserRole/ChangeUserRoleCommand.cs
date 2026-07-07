using MediatR;
using Patogh.Domain.Enums;

namespace Patogh.Application.Features.Admin.Commands.ChangeUserRole;

public class ChangeUserRoleCommand : IRequest<ChangeUserRoleResponseDto>
{
    public Guid UserId { get; set; }
    public UserRole NewRole { get; set; }
}

public class ChangeUserRoleResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}