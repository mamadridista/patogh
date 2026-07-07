using FluentValidation;

namespace Patogh.Application.Features.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandValidator
    : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام رستوران الزامی است.")
            .MaximumLength(200);

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("موقعیت مکانی الزامی است.");

        RuleFor(x => x.FoodType)
            .NotEmpty().WithMessage("نوع غذا الزامی است.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("زمان شروع باید قبل از زمان پایان باشد.");
    }
}