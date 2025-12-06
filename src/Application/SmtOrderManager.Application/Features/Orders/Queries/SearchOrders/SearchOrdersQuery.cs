using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Orders.Queries.SearchOrders;

public record SearchOrdersQuery(string SearchTerm) : IRequest<Result<IEnumerable<Order>>>;

public class SearchOrdersQueryHandler : IRequestHandler<SearchOrdersQuery, Result<IEnumerable<Order>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<SearchOrdersQueryHandler> _logger;

    public SearchOrdersQueryHandler(IOrderRepository orderRepository, ILogger<SearchOrdersQueryHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<Order>>> Handle(SearchOrdersQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("SearchOrdersQuery handling for term: {SearchTerm}", request.SearchTerm);
        }

        try
        {
            // Placeholder: repository has no search, reuse GetByIdsAsync until search implemented.
            var result = await _orderRepository.GetByIdsAsync(Enumerable.Empty<Guid>(), cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("SearchOrdersQuery handled with success: {Success}", result.Success);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling SearchOrdersQuery for term: {SearchTerm}", request.SearchTerm);
            return ex;
        }
    }
}
