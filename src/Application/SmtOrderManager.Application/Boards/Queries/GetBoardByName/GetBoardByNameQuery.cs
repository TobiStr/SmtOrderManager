using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Boards.Queries.GetBoardByName;

public record GetBoardByNameQuery(string Name) : IRequest<Result<Board>>;

public class GetBoardByNameQueryHandler : IRequestHandler<GetBoardByNameQuery, Result<Board>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<GetBoardByNameQueryHandler> _logger;

    public GetBoardByNameQueryHandler(IBoardRepository boardRepository, ILogger<GetBoardByNameQueryHandler> logger)
    {
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<Board>> Handle(GetBoardByNameQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
