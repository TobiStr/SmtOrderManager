using FluentValidation;

namespace SmtOrderManager.Application.Features.Components.Commands.DeleteComponent;

public class DeleteComponentCommandValidator : AbstractValidator<DeleteComponentCommand>
{
    public DeleteComponentCommandValidator()
    {
        RuleFor(x => x.ComponentId).NotEmpty();
    }
}
