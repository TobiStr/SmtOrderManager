using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Boards.Queries.GetAllBoards;

public record GetAllBoardsQuery : IRequest<Result<IEnumerable<Board>>>;

public class GetAllBoardsQueryHandler : IRequestHandler<GetAllBoardsQuery, Result<IEnumerable<Board>>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<GetAllBoardsQueryHandler> _logger;

    public GetAllBoardsQueryHandler(IBoardRepository boardRepository, ILogger<GetAllBoardsQueryHandler> logger)
    {
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<Board>>> Handle(GetAllBoardsQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetAllBoardsQuery handling");
        }

        try
        {
            var result = await _boardRepository.GetAllAsync(cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetAllBoardsQuery handled with success: {Success}", result.Success);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetAllBoardsQuery");
            return ex;
        }
    }
}
