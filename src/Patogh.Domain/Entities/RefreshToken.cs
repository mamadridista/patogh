using Patogh.Domain.Common;

namespace Patogh.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    // The token itself — a cryptographically random string
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    // For audit: which IP address created this token
    public string? CreatedByIp { get; set; }

    // For audit: which IP address revoked this token
    public string? RevokedByIp { get; set; }

    // If this token was replaced by a new one (token rotation)
    public string? ReplacedByToken { get; set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    // Navigation
    public User User { get; set; } = null!;
}