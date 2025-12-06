using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Orders.Queries.SearchOrders;

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

    public Task<Result<IEnumerable<Order>>> Handle(SearchOrdersQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
