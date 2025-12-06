using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<Order>>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<Order>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Order>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetOrderByIdQuery handling for Order ID: {OrderId}", request.OrderId);
        }

        try
        {
            var result = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetOrderByIdQuery handled with success: {Success} for Order ID: {OrderId}", result.Success, request.OrderId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetOrderByIdQuery for Order ID: {OrderId}", request.OrderId);
            return ex;
        }
    }
}
