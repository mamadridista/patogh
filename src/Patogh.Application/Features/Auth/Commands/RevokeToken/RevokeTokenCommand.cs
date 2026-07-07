using MediatR;

namespace Patogh.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommand : IRequest
{
    public string Token { get; set; } = string.Empty;
}