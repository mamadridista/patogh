using FluentValidation;

namespace Patogh.Application.Features.Tables.Commands.CreateTable;

public class CreateTableCommandValidator : AbstractValidator<CreateTableCommand>
{
    public CreateTableCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.TableNumber).GreaterThan(0);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(20);
    }
}