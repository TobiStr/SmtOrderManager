using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Boards.Commands.AddComponentToBoard;

public record AddComponentToBoardCommand(Guid BoardId, Guid ComponentId, long Quantity) : IRequest<Result<Board>>;

public class AddComponentToBoardCommandHandler : IRequestHandler<AddComponentToBoardCommand, Result<Board>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<AddComponentToBoardCommandHandler> _logger;

    public AddComponentToBoardCommandHandler(
        IBoardRepository boardRepository,
        ILogger<AddComponentToBoardCommandHandler> logger)
    {
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Board>> Handle(AddComponentToBoardCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("AddComponentToBoardCommand handling for Board ID: {BoardId}, Component ID: {ComponentId}", request.BoardId, request.ComponentId);
        }

        try
        {
            var boardResult = await _boardRepository.GetByIdAsync(request.BoardId, cancellationToken);
            if (!boardResult.Success)
            {
                return boardResult.GetError();
            }

            var board = boardResult.GetOk();

            if (board.ComponentIds.Any(x => x.Id == request.ComponentId))
            {
                var updated = board.UpdateComponentQuantity(request.ComponentId, request.Quantity);
                var saveExisting = await _boardRepository.AddOrUpdateAsync(updated, cancellationToken);
                if (!saveExisting.Success)
                {
                    return saveExisting.GetError();
                }

                return updated;
            }

            var updatedBoard = board.AddComponent(request.ComponentId, request.Quantity);

            var saveBoardResult = await _boardRepository.AddOrUpdateAsync(updatedBoard, cancellationToken);
            if (!saveBoardResult.Success)
            {
                return saveBoardResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("AddComponentToBoardCommand handled successfully for Board ID: {BoardId}", request.BoardId);
            }

            return updatedBoard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling AddComponentToBoardCommand for Board ID: {BoardId}", request.BoardId);
            return ex;
        }
    }
}
