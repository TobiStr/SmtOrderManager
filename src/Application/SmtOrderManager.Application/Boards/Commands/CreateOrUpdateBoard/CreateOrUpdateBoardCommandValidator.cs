using FluentValidation;

namespace SmtOrderManager.Application.Boards.Commands.CreateOrUpdateBoard;

public class CreateOrUpdateBoardCommandValidator : AbstractValidator<CreateOrUpdateBoardCommand>
{
    public CreateOrUpdateBoardCommandValidator()
    {
    }
}
