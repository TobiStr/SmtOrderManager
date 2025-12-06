using FluentValidation;

namespace SmtOrderManager.Application.Boards.Commands.AddComponentToBoard;

public class AddComponentToBoardCommandValidator : AbstractValidator<AddComponentToBoardCommand>
{
    public AddComponentToBoardCommandValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.ComponentId).NotEmpty();
    }
}
