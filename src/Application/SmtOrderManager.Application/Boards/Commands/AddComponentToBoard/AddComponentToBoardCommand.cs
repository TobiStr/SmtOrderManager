using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Boards.Commands.AddComponentToBoard;

public record AddComponentToBoardCommand(Guid BoardId, Component Component) : IRequest<Result<Board>>;

public class AddComponentToBoardCommandHandler : IRequestHandler<AddComponentToBoardCommand, Result<Board>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IComponentRepository _componentRepository;
    private readonly ILogger<AddComponentToBoardCommandHandler> _logger;

    public AddComponentToBoardCommandHandler(
        IBoardRepository boardRepository,
        IComponentRepository componentRepository,
        ILogger<AddComponentToBoardCommandHandler> logger)
    {
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<Board>> Handle(AddComponentToBoardCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
