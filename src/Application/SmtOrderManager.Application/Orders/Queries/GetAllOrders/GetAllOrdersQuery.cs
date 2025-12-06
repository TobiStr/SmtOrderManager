using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Orders.Queries.GetAllOrders;

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

    public Task<Result<IEnumerable<Order>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
