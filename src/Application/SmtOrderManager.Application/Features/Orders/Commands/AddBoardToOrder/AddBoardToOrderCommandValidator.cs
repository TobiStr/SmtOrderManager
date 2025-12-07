using FluentValidation;

namespace SmtOrderManager.Application.Features.Orders.Commands.AddBoardToOrder;

public class AddBoardToOrderCommandValidator : AbstractValidator<AddBoardToOrderCommand>
{
    public AddBoardToOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.BoardId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
