using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Boards.Queries.GetBoardById;

public record GetBoardByIdQuery(Guid BoardId) : IRequest<Result<Board>>;

public class GetBoardByIdQueryHandler : IRequestHandler<GetBoardByIdQuery, Result<Board>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<GetBoardByIdQueryHandler> _logger;

    public GetBoardByIdQueryHandler(IBoardRepository boardRepository, ILogger<GetBoardByIdQueryHandler> logger)
    {
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<Board>> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
