using FluentValidation;

namespace SmtOrderManager.Application.Users.Commands.AddOrderToUser;

public class AddOrderToUserCommandValidator : AbstractValidator<AddOrderToUserCommand>
{
    public AddOrderToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
