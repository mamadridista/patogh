namespace Patogh.Application.Configurations;

/// <summary>
/// JWT configuration options. Moved to Application layer to avoid
/// a circular dependency (Application → Infrastructure is forbidden).
/// Infrastructure reads this same type via the same namespace.
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}