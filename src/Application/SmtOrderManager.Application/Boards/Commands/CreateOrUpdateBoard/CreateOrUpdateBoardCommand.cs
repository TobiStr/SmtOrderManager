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

    public Task<Result<Board>> Handle(CreateOrUpdateBoardCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
