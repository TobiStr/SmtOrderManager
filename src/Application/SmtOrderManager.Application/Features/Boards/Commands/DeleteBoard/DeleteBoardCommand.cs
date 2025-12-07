using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Boards.Commands.DeleteBoard;

public record DeleteBoardCommand(Guid BoardId) : IRequest<Result>;

public class DeleteBoardCommandHandler : IRequestHandler<DeleteBoardCommand, Result>
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<DeleteBoardCommandHandler> _logger;

    public DeleteBoardCommandHandler(IBoardRepository boardRepository, ILogger<DeleteBoardCommandHandler> logger)
    {
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("DeleteBoardCommand handling for Board ID: {BoardId}", request.BoardId);
        }

        try
        {
            var deleteResult = await _boardRepository.DeleteAsync(request.BoardId, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteBoardCommand handled with success: {Success} for Board ID: {BoardId}", deleteResult.Success, request.BoardId);
            }

            return deleteResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling DeleteBoardCommand for Board ID: {BoardId}", request.BoardId);
            return ex;
        }
    }
}
