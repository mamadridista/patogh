using MediatR;
using Microsoft.Extensions.Options;
using Patogh.Application.Features.Auth.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Application.Configurations;
using Microsoft.AspNetCore.Http;

namespace Patogh.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        IHttpContextAccessor httpContextAccessor)
    {
        _refreshTokenService = refreshTokenService;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LoginResponseDto> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var ip = _httpContextAccessor.HttpContext?
            .Connection.RemoteIpAddress?.ToString();

        var (user, newRefreshToken) = await _refreshTokenService
            .RotateRefreshTokenAsync(request.Token, ip, cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            UserId = user.Id,
            ExpiresInMinutes = _jwtSettings.ExpirationInMinutes
        };
    }
}