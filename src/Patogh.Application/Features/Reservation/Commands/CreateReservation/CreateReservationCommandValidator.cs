using FluentValidation;

namespace Patogh.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidator
    : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.RestaurantId)
            .NotEmpty().WithMessage("شناسه رستوران الزامی است.");

        RuleFor(x => x.TableId)
            .NotEmpty().WithMessage("شناسه میز الزامی است.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("نام رزروکننده الزامی است.")
            .MaximumLength(150);

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("شماره تلفن رزروکننده الزامی است.")
            .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره تلفن صحیح نیست.");

        RuleFor(x => x.GuestCount)
            .GreaterThan(0).WithMessage("تعداد نفرات باید بیشتر از صفر باشد.")
            .LessThanOrEqualTo(20).WithMessage("تعداد نفرات نمی‌تواند بیشتر از ۲۰ باشد.");

        RuleFor(x => x.ReservationDate)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("تاریخ رزرو نمی‌تواند در گذشته باشد.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("زمان شروع الزامی است.");
    }
}