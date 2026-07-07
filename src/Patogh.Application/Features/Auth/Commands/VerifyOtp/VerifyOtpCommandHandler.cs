using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Patogh.Application.Features.Auth.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;
using Patogh.Application.Configurations;

namespace Patogh.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _http;

    public VerifyOtpCommandHandler(
        IApplicationDbContext context,
        IOtpService otpService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IOptions<JwtSettings> jwtSettings,
        IHttpContextAccessor http)
    {
        _context = context;
        _otpService = otpService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _jwtSettings = jwtSettings.Value;
        _http = http;
    }

    public async Task<LoginResponseDto> Handle(
        VerifyOtpCommand request,
        CancellationToken cancellationToken)
    {
        var isValid = await _otpService.VerifyOtpAsync(
            request.PhoneNumber, request.Code);

        if (!isValid)
            throw new DomainValidationException(
                "کد تأیید نادرست یا منقضی شده است.");

        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.PhoneNumber == request.PhoneNumber,
                cancellationToken);

        if (user is null)
            throw new NotFoundException("User", request.PhoneNumber);

        if (request.RequestedRole == "RestaurantOwner" &&
            user.Role == UserRole.Customer)
        {
            user.Role = UserRole.RestaurantOwner;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var refreshToken = await _refreshTokenService
            .CreateRefreshTokenAsync(user.Id, ip, cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            UserId = user.Id,
            ExpiresInMinutes = _jwtSettings.ExpirationInMinutes
        };
    }
}