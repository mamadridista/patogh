using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Patogh.Application.Features.Auth.Commands.AdminLogin;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/admin/auth")]
[EnableRateLimiting("auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>ورود اختصاصی ادمین با شماره تلفن و رمز عبور</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(AdminLoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}