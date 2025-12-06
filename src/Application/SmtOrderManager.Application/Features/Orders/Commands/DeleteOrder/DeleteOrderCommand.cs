using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(Guid OrderId) : IRequest<Result>;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ILogger<DeleteOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("DeleteOrderCommand handling for Order ID: {OrderId}", request.OrderId);
        }

        try
        {
            var deleteResult = await _orderRepository.DeleteAsync(request.OrderId, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteOrderCommand handled with success: {Success} for Order ID: {OrderId}", deleteResult.Success, request.OrderId);
            }

            return deleteResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling DeleteOrderCommand for Order ID: {OrderId}", request.OrderId);
            return ex;
        }
    }
}
