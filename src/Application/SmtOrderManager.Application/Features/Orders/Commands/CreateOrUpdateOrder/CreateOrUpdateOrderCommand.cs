using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Orders.Commands.CreateOrUpdateOrder;

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

    public async Task<Result<Order>> Handle(CreateOrUpdateOrderCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("CreateOrUpdateOrderCommand handling for Order ID: {OrderId}", request.Order.Id);
        }

        try
        {
            var upsertResult = await _orderRepository.AddOrUpdateAsync(request.Order, cancellationToken);
            if (!upsertResult.Success)
            {
                return upsertResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("CreateOrUpdateOrderCommand handled successfully for Order ID: {OrderId}", request.Order.Id);
            }

            return request.Order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CreateOrUpdateOrderCommand for Order ID: {OrderId}", request.Order.Id);
            return ex;
        }
    }
}
