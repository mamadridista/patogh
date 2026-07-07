using MediatR;
using Microsoft.EntityFrameworkCore;
using Patogh.Application.Features.Auth.DTOs;
using Patogh.Application.Interfaces;
using Patogh.Domain.Enums;
using Patogh.Domain.Exceptions;

namespace Patogh.Application.Features.Auth.Commands.AdminLogin;

public class AdminLoginCommandHandler : IRequestHandler<AdminLoginCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AdminLoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> Handle(
        AdminLoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.PhoneNumber == request.PhoneNumber,
                cancellationToken);

        if (user is null)
            throw new Exception();

        var isValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!isValid)
            throw new UnauthorizedDomainException();

        var token = _jwtTokenService.GenerateAccessToken(user);

        return new LoginResponseDto
        {
            AccessToken = token,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            UserId = user.Id
        };
    }
}