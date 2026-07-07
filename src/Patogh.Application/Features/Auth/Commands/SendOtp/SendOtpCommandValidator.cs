using FluentValidation;

namespace Patogh.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("شماره تلفن الزامی است.")
            .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره تلفن صحیح نیست.");
    }
}