using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Orders.Commands.AddBoardToOrder;

public record AddBoardToOrderCommand(Guid OrderId, Guid BoardId) : IRequest<Result<Order>>;

public class AddBoardToOrderCommandHandler : IRequestHandler<AddBoardToOrderCommand, Result<Order>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AddBoardToOrderCommandHandler> _logger;

    public AddBoardToOrderCommandHandler(
        IOrderRepository orderRepository,
        ILogger<AddBoardToOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Order>> Handle(AddBoardToOrderCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("AddBoardToOrderCommand handling for Order ID: {OrderId}, Board ID: {BoardId}", request.OrderId, request.BoardId);
        }

        try
        {
            var orderResult = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (!orderResult.Success)
            {
                return orderResult.GetError();
            }

            var order = orderResult.GetOk();
            var updatedOrder = order.BoardIds.Contains(request.BoardId)
                ? order
                : order with { BoardIds = order.BoardIds.Concat(new[] { request.BoardId }).ToList() };

            var saveOrderResult = await _orderRepository.AddOrUpdateAsync(updatedOrder, cancellationToken);
            if (!saveOrderResult.Success)
            {
                return saveOrderResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("AddBoardToOrderCommand handled successfully for Order ID: {OrderId}", request.OrderId);
            }

            return updatedOrder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling AddBoardToOrderCommand for Order ID: {OrderId}", request.OrderId);
            return ex;
        }
    }
}
