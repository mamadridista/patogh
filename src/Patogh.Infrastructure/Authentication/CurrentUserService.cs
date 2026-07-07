using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Patogh.Application.Interfaces;

namespace Patogh.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(value))
                return Guid.Empty;

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string PhoneNumber =>
        User?.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty;

    public string Role =>
        User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}