using FluentValidation;

namespace SmtOrderManager.Application.Features.Boards.Commands.DeleteBoard;

public class DeleteBoardCommandValidator : AbstractValidator<DeleteBoardCommand>
{
    public DeleteBoardCommandValidator()
    {
        RuleFor(x => x.BoardId).NotEmpty();
    }
}
