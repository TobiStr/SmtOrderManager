using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Orders.Commands.CreateOrUpdateOrder;

public record CreateOrUpdateOrderCommand(Order Order) : IRequest<Result<Order>>;

public class CreateOrUpdateOrderCommandHandler : IRequestHandler<CreateOrUpdateOrderCommand, Result<Order>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CreateOrUpdateOrderCommandHandler> _logger;

    public CreateOrUpdateOrderCommandHandler(IOrderRepository orderRepository, ILogger<CreateOrUpdateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<Order>> Handle(CreateOrUpdateOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
