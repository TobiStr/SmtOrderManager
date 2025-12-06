using FluentValidation;

namespace SmtOrderManager.Application.Features.Components.Commands.CreateOrUpdateComponent;

public class CreateOrUpdateComponentCommandValidator : AbstractValidator<CreateOrUpdateComponentCommand>
{
    public CreateOrUpdateComponentCommandValidator()
    {
        RuleFor(x => x.Component).NotNull();
        RuleFor(x => x.Component.Name).NotEmpty();
        RuleFor(x => x.Component.Description).NotEmpty();
        RuleFor(x => x.Component.Quantity).GreaterThan(0);
        RuleFor(x => x.Component.BoardId).NotEmpty();
    }
}
