using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Boards.Queries.GetBoardByName;

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

    public async Task<Result<Board>> Handle(GetBoardByNameQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetBoardByNameQuery handling for Board name: {BoardName}", request.Name);
        }

        try
        {
            var result = await _boardRepository.GetByNameAsync(request.Name, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetBoardByNameQuery handled with success: {Success} for Board name: {BoardName}", result.Success, request.Name);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetBoardByNameQuery for Board name: {BoardName}", request.Name);
            return ex;
        }
    }
}
