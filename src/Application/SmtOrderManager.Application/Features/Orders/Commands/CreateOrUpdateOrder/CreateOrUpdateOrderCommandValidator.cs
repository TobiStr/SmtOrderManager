using FluentValidation;

namespace SmtOrderManager.Application.Features.Orders.Commands.CreateOrUpdateOrder;

public class CreateOrUpdateOrderCommandValidator : AbstractValidator<CreateOrUpdateOrderCommand>
{
    public CreateOrUpdateOrderCommandValidator()
    {
        RuleFor(x => x.Order).NotNull();
        RuleFor(x => x.Order.Description).NotEmpty();
        RuleFor(x => x.Order.OrderDate).NotEqual(default(DateTime));
        RuleFor(x => x.Order.UserId).NotEmpty();
    }
}
