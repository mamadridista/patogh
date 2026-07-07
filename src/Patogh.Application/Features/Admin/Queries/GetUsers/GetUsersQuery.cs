using MediatR;
using Patogh.Application.Common.Models;
using Patogh.Application.Features.Admin.DTOs;

namespace Patogh.Application.Features.Admin.Queries.GetUsers;

public class GetUsersQuery : PagedQuery, IRequest<PaginatedResult<AdminUserDto>>
{
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
}