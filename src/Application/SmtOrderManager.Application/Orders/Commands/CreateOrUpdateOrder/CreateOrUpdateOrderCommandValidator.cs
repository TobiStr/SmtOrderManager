using FluentValidation;

namespace SmtOrderManager.Application.Orders.Commands.CreateOrUpdateOrder;

public class CreateOrUpdateOrderCommandValidator : AbstractValidator<CreateOrUpdateOrderCommand>
{
    public CreateOrUpdateOrderCommandValidator()
    {
    }
}
