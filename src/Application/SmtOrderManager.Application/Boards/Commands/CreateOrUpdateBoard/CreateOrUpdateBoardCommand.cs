using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Boards.Commands.CreateOrUpdateBoard;

public record CreateOrUpdateBoardCommand(Board Board) : IRequest<Result<Board>>;

public class CreateOrUpdateBoardCommandHandler : IRequestHandler<CreateOrUpdateBoardCommand, Result<Board>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<CreateOrUpdateBoardCommandHandler> _logger;

    public CreateOrUpdateBoardCommandHandler(IBoardRepository boardRepository, ILogger<CreateOrUpdateBoardCommandHandler> logger)
    {
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Board>> Handle(CreateOrUpdateBoardCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("CreateOrUpdateBoardCommand handling for Board ID: {BoardId}", request.Board.Id);
        }

        try
        {
            var upsertResult = await _boardRepository.AddOrUpdateAsync(request.Board, cancellationToken);
            if (!upsertResult.Success)
            {
                return upsertResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("CreateOrUpdateBoardCommand handled successfully for Board ID: {BoardId}", request.Board.Id);
            }

            return request.Board;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CreateOrUpdateBoardCommand for Board ID: {BoardId}", request.Board.Id);
            return ex;
        }
    }
}
