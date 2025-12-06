using FluentValidation;

namespace SmtOrderManager.Application.Components.Commands.DeleteComponent;

public class DeleteComponentCommandValidator : AbstractValidator<DeleteComponentCommand>
{
    public DeleteComponentCommandValidator()
    {
    }
}
