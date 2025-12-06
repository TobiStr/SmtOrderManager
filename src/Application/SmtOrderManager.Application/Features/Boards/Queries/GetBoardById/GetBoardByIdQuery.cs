using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Boards.Queries.GetBoardById;

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

    public async Task<Result<Board>> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetBoardByIdQuery handling for Board ID: {BoardId}", request.BoardId);
        }

        try
        {
            var result = await _boardRepository.GetByIdAsync(request.BoardId, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetBoardByIdQuery handled with success: {Success} for Board ID: {BoardId}", result.Success, request.BoardId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetBoardByIdQuery for Board ID: {BoardId}", request.BoardId);
            return ex;
        }
    }
}
