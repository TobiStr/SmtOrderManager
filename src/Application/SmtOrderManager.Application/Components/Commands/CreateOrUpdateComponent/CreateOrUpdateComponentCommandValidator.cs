using FluentValidation;

namespace SmtOrderManager.Application.Components.Commands.CreateOrUpdateComponent;

public class CreateOrUpdateComponentCommandValidator : AbstractValidator<CreateOrUpdateComponentCommand>
{
    public CreateOrUpdateComponentCommandValidator()
    {
    }
}
