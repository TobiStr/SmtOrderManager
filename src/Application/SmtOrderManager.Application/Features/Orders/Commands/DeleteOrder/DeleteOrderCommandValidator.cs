using FluentValidation;

namespace SmtOrderManager.Application.Features.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
