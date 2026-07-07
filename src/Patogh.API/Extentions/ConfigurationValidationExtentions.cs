namespace Patogh.API.Extensions;

public static class ConfigurationValidationExtensions
{
    /// <summary>
    /// Validates critical configuration values on startup.
    /// The application will throw and refuse to start if any required
    /// value is missing or insecure. This is "fail fast" — better to
    /// crash at startup than to fail silently at runtime.
    /// </summary>
    public static void ValidateConfiguration(this WebApplication app)
    {
        var config = app.Configuration;

        // ── JWT Secret ────────────────────────────────────────────────────────
        var jwtSecret = config["JwtSettings:Secret"];

        if (string.IsNullOrWhiteSpace(jwtSecret))
            throw new InvalidOperationException(
                "STARTUP FAILURE: JwtSettings:Secret is not configured. " +
                "Set it via user-secrets, environment variable, or appsettings.Development.json.");

        if (jwtSecret.Contains("REPLACE_IN_USER_SECRETS") ||
            jwtSecret.Contains("DEVELOPMENT_SECRET"))
            throw new InvalidOperationException(
                "STARTUP FAILURE: JwtSettings:Secret contains a placeholder value. " +
                "Configure a real secret with at least 32 characters.");

        if (jwtSecret.Length < 32)
            throw new InvalidOperationException(
                $"STARTUP FAILURE: JwtSettings:Secret must be at least 32 characters. " +
                $"Current length: {jwtSecret.Length}.");

        // ── Database Connection ───────────────────────────────────────────────
        var dbConn = config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(dbConn))
            throw new InvalidOperationException(
                "STARTUP FAILURE: ConnectionStrings:DefaultConnection is not configured.");

        if (dbConn.Contains("REPLACE_IN_USER_SECRETS"))
            throw new InvalidOperationException(
                "STARTUP FAILURE: ConnectionStrings:DefaultConnection contains a placeholder value.");

        app.Logger.LogInformation("Configuration validation passed.");
    }
}