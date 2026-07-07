using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Patogh.Application.Interfaces;
using Patogh.Domain.Entities;
using Patogh.Domain.Enums;

namespace Patogh.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, SendOtpResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IHostEnvironment _env;

    // The magic OTP accepted by MockOtpService is defined here as a constant
    // so SendOtpCommandHandler and MockOtpService use the same value.
    // If you change MockOtpService's accepted code, change this too.
    internal const string DevMagicOtp = "123456";

    public SendOtpCommandHandler(
        IApplicationDbContext context,
        IOtpService otpService,
        IHostEnvironment env)
    {
        _context = context;
        _otpService = otpService;
        _env = env;
    }

    public async Task<SendOtpResponseDto> Handle(
        SendOtpCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.PhoneNumber == request.PhoneNumber,
                cancellationToken);

        if (user is null)
        {
            user = new User
            {
                PhoneNumber = request.PhoneNumber,
                PasswordHash = string.Empty,
                Role = UserRole.Customer
            };
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _otpService.SendOtpAsync(request.PhoneNumber);

        var response = new SendOtpResponseDto
        {
            Success = true,
            Message = $"کد تأیید به شماره {request.PhoneNumber} ارسال شد."
        };

        // DevOtp is only exposed when running with Development environment.
        // This tells the developer which code to use (MockOtpService always
        // accepts DevMagicOtp; the value is printed to the console too).
        // In Production, DevOtp remains null and is never serialized.
        if (_env.IsDevelopment())
            response.DevOtp = DevMagicOtp;

        return response;
    }
}