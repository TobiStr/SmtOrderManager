using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery : IRequest<Result<IEnumerable<Order>>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<IEnumerable<Order>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetAllOrdersQueryHandler> _logger;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository, ILogger<GetAllOrdersQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<Order>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetAllOrdersQuery handling");
        }

        try
        {
            var result = await _orderRepository.GetByIdsAsync(Enumerable.Empty<Guid>(), cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetAllOrdersQuery handled with success: {Success}", result.Success);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetAllOrdersQuery");
            return ex;
        }
    }
}
