using MediatR;
using Patogh.Application.Features.Auth.DTOs;

namespace Patogh.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<LoginResponseDto>
{
    public string Token { get; set; } = string.Empty;
}