using MediatR;
using Patogh.Application.Features.Auth.DTOs;

namespace Patogh.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<LoginResponseDto>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}