using FluentValidation;

namespace SmtOrderManager.Application.Boards.Commands.AddComponentToBoard;

public class AddComponentToBoardCommandValidator : AbstractValidator<AddComponentToBoardCommand>
{
    public AddComponentToBoardCommandValidator()
    {
    }
}
