using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Orders.Commands.AddBoardToOrder;

public record AddBoardToOrderCommand(Guid OrderId, Board Board) : IRequest<Result<Order>>;

public class AddBoardToOrderCommandHandler : IRequestHandler<AddBoardToOrderCommand, Result<Order>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<AddBoardToOrderCommandHandler> _logger;

    public AddBoardToOrderCommandHandler(
        IOrderRepository orderRepository,
        IBoardRepository boardRepository,
        ILogger<AddBoardToOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<Order>> Handle(AddBoardToOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
