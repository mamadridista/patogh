using MediatR;
using Microsoft.AspNetCore.Http;
using Patogh.Application.Interfaces;

namespace Patogh.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RevokeTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IHttpContextAccessor httpContextAccessor)
    {
        _refreshTokenService = refreshTokenService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Handle(
        RevokeTokenCommand request,
        CancellationToken cancellationToken)
    {
        var ip = _httpContextAccessor.HttpContext?
            .Connection.RemoteIpAddress?.ToString();

        await _refreshTokenService.RevokeTokenAsync(
            request.Token, ip, cancellationToken);
    }
}