using FluentValidation;

namespace SmtOrderManager.Application.Features.Users.Commands.AddOrderToUser;

public class AddOrderToUserCommandValidator : AbstractValidator<AddOrderToUserCommand>
{
    public AddOrderToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
