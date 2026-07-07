using Patogh.Domain.Entities;

namespace Patogh.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
}