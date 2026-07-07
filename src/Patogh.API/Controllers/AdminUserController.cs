using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Patogh.Application.Features.Admin.Commands.ChangeUserRole;
using Patogh.Application.Features.Admin.Queries.GetUsers;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("general")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// لیست کاربران با صفحه‌بندی و فیلتر.
    /// Query: phoneNumber, role (Customer|RestaurantOwner|Admin), page, pageSize
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// تغییر نقش کاربر.
    /// Admin نمی‌تواند نقش Admin بدهد یا از Admin بگیرد.
    /// </summary>
    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(
        Guid id, [FromBody] ChangeUserRoleCommand command)
    {
        command.UserId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}