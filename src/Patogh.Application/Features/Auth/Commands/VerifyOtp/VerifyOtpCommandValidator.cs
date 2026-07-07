using FluentValidation;

namespace Patogh.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره تلفن صحیح نیست.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد تأیید الزامی است.")
            .Length(6).WithMessage("کد تأیید باید ۶ رقم باشد.")
            .Matches(@"^\d{6}$").WithMessage("کد تأیید باید فقط شامل اعداد باشد.");
    }
}