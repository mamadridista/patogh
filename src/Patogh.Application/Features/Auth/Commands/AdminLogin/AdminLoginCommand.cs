using MediatR;
using Patogh.Application.Features.Auth.DTOs;

namespace Patogh.Application.Features.Auth.Commands.AdminLogin;

public class AdminLoginCommand : IRequest<LoginResponseDto>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}