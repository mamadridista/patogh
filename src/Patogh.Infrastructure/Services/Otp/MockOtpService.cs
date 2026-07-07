using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Patogh.Application.Interfaces;
using Patogh.Infrastructure.Configurations;

namespace Patogh.Infrastructure.Services.Otp;

/// <summary>
/// In-memory OTP service used when Redis is not configured.
/// Accepts a fixed code (configured via OtpSettings:DevOtpCode, default "123456")
/// so developers can test OTP flows via Swagger without a real phone.
///
/// NEVER register this in Production — it accepts any request with the dev code.
/// </summary>
public class MockOtpService : IOtpService
{
    private readonly ILogger<MockOtpService> _logger;
    private readonly string _devCode;

    public MockOtpService(
        ILogger<MockOtpService> logger,
        IOptions<OtpSettings> settings)
    {
        _logger = logger;
        _devCode = settings.Value.DevOtpCode;
    }

    public Task SendOtpAsync(string phoneNumber)
    {
        _logger.LogWarning(
            "[MOCK OTP] ⚠ Development mode — use '{Code}' to verify {Phone}. " +
            "This service MUST NOT run in Production.",
            _devCode, phoneNumber);
        return Task.CompletedTask;
    }

    public Task<bool> VerifyOtpAsync(string phoneNumber, string code)
    {
        var isValid = code == _devCode;
        _logger.LogInformation(
            "[MOCK OTP] Verify {Code} for {Phone}: {Result}",
            code, phoneNumber, isValid ? "ACCEPTED" : "REJECTED");
        return Task.FromResult(isValid);
    }
}
