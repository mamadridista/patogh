using MediatR;
using Patogh.Application.Features.Auth.DTOs;

namespace Patogh.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommand : IRequest<LoginResponseDto>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Optional role requested during first-time registration via OTP.
    /// Only RestaurantOwner is accepted — Customer is default.
    /// Admin can never be created via this endpoint.
    /// </summary>
    public string? RequestedRole { get; set; }
}