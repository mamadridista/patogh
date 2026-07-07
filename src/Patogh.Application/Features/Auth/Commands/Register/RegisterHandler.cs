using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Auth.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResponseDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _context.Users
            .AnyAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);

        if (exists)
            throw new ConflictException("این شماره تلفن قبلاً ثبت شده است.");

        // Admin cannot be created via API
        var role = request.Role == UserRole.RestaurantOwner
            ? UserRole.RestaurantOwner
            : UserRole.Customer;

        var user = new User
        {
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = role
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterResponseDto
        {
            Success = true,
            UserId = user.Id,
            Message = "ثبت‌نام با موفقیت انجام شد."
        };
    }
}