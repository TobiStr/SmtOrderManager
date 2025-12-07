using FluentValidation;

namespace SmtOrderManager.Application.Features.Boards.Commands.CreateOrUpdateBoard;

public class CreateOrUpdateBoardCommandValidator : AbstractValidator<CreateOrUpdateBoardCommand>
{
    public CreateOrUpdateBoardCommandValidator()
    {
        RuleFor(x => x.Board).NotNull();
        RuleFor(x => x.Board.Name).NotEmpty();
        RuleFor(x => x.Board.Description).NotEmpty();
        RuleFor(x => x.Board.Length).GreaterThan(0);
        RuleFor(x => x.Board.Width).GreaterThan(0);
    }
}
