namespace Patogh.Infrastructure.Configurations;

/// <summary>
/// Strongly-typed OTP configuration. Values are bound from appsettings.json
/// section "OtpSettings". Magic numbers eliminated.
/// </summary>
public class OtpSettings
{
    /// <summary>How long (in minutes) an OTP remains valid. Default: 3.</summary>
    public int ExpiryMinutes { get; set; } = 2;

    /// <summary>
    /// The fixed OTP code accepted by MockOtpService in Development.
    /// Must never be used in Production. Default: "123456".
    /// </summary>
    public string DevOtpCode { get; set; } = "123456";
}
