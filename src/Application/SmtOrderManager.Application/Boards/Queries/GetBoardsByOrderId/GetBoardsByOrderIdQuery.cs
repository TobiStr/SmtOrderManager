using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Boards.Queries.GetBoardsByOrderId;

public record GetBoardsByOrderIdQuery(Guid OrderId) : IRequest<Result<IEnumerable<Board>>>;

public class GetBoardsByOrderIdQueryHandler : IRequestHandler<GetBoardsByOrderIdQuery, Result<IEnumerable<Board>>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<GetBoardsByOrderIdQueryHandler> _logger;

    public GetBoardsByOrderIdQueryHandler(IBoardRepository boardRepository, ILogger<GetBoardsByOrderIdQueryHandler> logger)
    {
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<IEnumerable<Board>>> Handle(GetBoardsByOrderIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
