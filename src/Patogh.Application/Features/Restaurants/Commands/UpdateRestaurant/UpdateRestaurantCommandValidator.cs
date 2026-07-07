using FluentValidation;

namespace Patogh.Application.Features.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommandValidator
    : AbstractValidator<UpdateRestaurantCommand>
{
    public UpdateRestaurantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام رستوران الزامی است.")
            .MaximumLength(200);

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("موقعیت مکانی الزامی است.")
            .MaximumLength(500);

        RuleFor(x => x.FoodType)
            .NotEmpty().WithMessage("نوع غذا الزامی است.")
            .MaximumLength(100);

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("زمان شروع باید قبل از زمان پایان باشد.");
    }
}