using Patogh.Domain.Entities;

namespace Patogh.Application.Interfaces;

public interface IRefreshTokenService
{
    /// <summary>Generates a cryptographically secure refresh token string.</summary>
    string GenerateToken();

    /// <summary>Creates and persists a RefreshToken for the given user.</summary>
    Task<RefreshToken> CreateRefreshTokenAsync(
        Guid userId,
        string? ipAddress,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates the token, marks it as revoked, creates a replacement,
    /// and returns the user + new token. Throws if invalid or expired.
    /// </summary>
    Task<(User user, RefreshToken newToken)> RotateRefreshTokenAsync(
        string token,
        string? ipAddress,
        CancellationToken cancellationToken);

    /// <summary>Revokes a specific token (logout).</summary>
    Task RevokeTokenAsync(
        string token,
        string? ipAddress,
        CancellationToken cancellationToken);
}