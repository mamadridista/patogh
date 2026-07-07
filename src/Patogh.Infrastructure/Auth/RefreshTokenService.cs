using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;
using Patogh.Domain.Exceptions;
using Patogh.Infrastructure.Configurations;
using System.Security.Cryptography;

namespace Patogh.Infrastructure.Auth;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IApplicationDbContext _context;
    private readonly JwtSettings _settings;

    public RefreshTokenService(
        IApplicationDbContext context,
        IOptions<JwtSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public string GenerateToken()
    {
        // 64 bytes = 512 bits of entropy — cryptographically secure
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(
        Guid userId,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _settings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };

        await _context.RefreshTokens.AddAsync(token, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<(User user, RefreshToken newToken)> RotateRefreshTokenAsync(
        string token,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (refreshToken is null)
            throw new DomainValidationException("توکن نامعتبر است.");

        if (refreshToken.IsRevoked)
        {
            // Detected reuse of a revoked token — potential token theft.
            // Revoke all tokens for this user as a security measure.
            await RevokeAllUserTokensAsync(
                refreshToken.UserId, ipAddress, cancellationToken);

            throw new DomainValidationException(
                "توکن باطل شده است. لطفاً دوباره وارد شوید.");
        }

        if (!refreshToken.IsActive)
            throw new DomainValidationException("توکن منقضی شده است.");

        // Token rotation: mark old token as replaced, issue new one
        var newToken = new RefreshToken
        {
            UserId = refreshToken.UserId,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _settings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReplacedByToken = newToken.Token;

        await _context.RefreshTokens.AddAsync(newToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return (refreshToken.User, newToken);
    }

    public async Task RevokeTokenAsync(
        string token,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
            throw new DomainValidationException("توکن نامعتبر یا منقضی است.");

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeAllUserTokensAsync(
        Guid userId,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var t in activeTokens)
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedByIp = ipAddress;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}