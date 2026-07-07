using MediatR;

namespace Patogh.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommand : IRequest<SendOtpResponseDto>
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class SendOtpResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    // In production: never return the OTP. Only in Development for testing.
    public string? DevOtp { get; set; }
}