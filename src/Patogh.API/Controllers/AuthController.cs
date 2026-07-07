using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Patogh.Application.Features.Auth.Commands.Login;
using Patogh.Application.Features.Auth.Commands.RefreshToken;
using Patogh.Application.Features.Auth.Commands.Register;
using Patogh.Application.Features.Auth.Commands.RevokeToken;
using Patogh.Application.Features.Auth.Commands.SendOtp;
using Patogh.Application.Features.Auth.Commands.VerifyOtp;

namespace Patogh.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [Primary] ارسال کد OTP به شماره تلفن.
    /// اگر کاربر جدید باشد، حساب Customer ساخته می‌شود.
    /// </summary>
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(SendOtpCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// [Primary] تأیید OTP و دریافت Access Token + Refresh Token.
    /// RequestedRole: "RestaurantOwner" برای ثبت‌نام صاحب رستوران.
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>[Secondary] ورود با رمز عبور</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>[Legacy] ثبت‌نام با رمز عبور</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// تجدید Access Token با استفاده از Refresh Token.
    /// Refresh Token قدیمی باطل می‌شود و یک توکن جدید صادر می‌شود (Token Rotation).
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// باطل کردن Refresh Token (Logout).
    /// پس از این عملیات، توکن دیگر قابل استفاده نیست.
    /// </summary>
    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RevokeTokenCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Success = true, Message = "توکن با موفقیت باطل شد." });
    }
}