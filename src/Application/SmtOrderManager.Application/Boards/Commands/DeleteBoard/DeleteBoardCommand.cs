using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Boards.Commands.DeleteBoard;

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

    public Task<Result> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
