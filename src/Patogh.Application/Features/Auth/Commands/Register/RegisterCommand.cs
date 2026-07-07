using MediatR;
using Patogh.Application.Features.Auth.DTOs;
using Patogh.Domain.Enums;

namespace Patogh.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<RegisterResponseDto>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Only Customer or RestaurantOwner can self-register
    // Admin is never created via API
    public UserRole Role { get; set; } = UserRole.Customer;
}