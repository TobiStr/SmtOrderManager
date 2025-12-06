using FluentValidation;

namespace SmtOrderManager.Application.Orders.Commands.AddBoardToOrder;

public class AddBoardToOrderCommandValidator : AbstractValidator<AddBoardToOrderCommand>
{
    public AddBoardToOrderCommandValidator()
    {
    }
}
