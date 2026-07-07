using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Patogh.Application.Features.Auth.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Exceptions;
using Patogh.Application.Configurations;

namespace Patogh.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _http;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IOptions<JwtSettings> jwtSettings,
        IHttpContextAccessor http)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _jwtSettings = jwtSettings.Value;
        _http = http;
    }

    public async Task<LoginResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.PhoneNumber == request.PhoneNumber,
                cancellationToken);

        if (user is null)
            throw new UnauthorizedDomainException();

        if (string.IsNullOrEmpty(user.PasswordHash) ||
            !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedDomainException();

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