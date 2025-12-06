using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Components.Queries.GetComponentsByBoardId;

public record GetComponentsByBoardIdQuery(Guid BoardId) : IRequest<Result<IEnumerable<Component>>>;

public class GetComponentsByBoardIdQueryHandler : IRequestHandler<GetComponentsByBoardIdQuery, Result<IEnumerable<Component>>>
{
    private readonly IComponentRepository _componentRepository;
    private readonly ILogger<GetComponentsByBoardIdQueryHandler> _logger;

    public GetComponentsByBoardIdQueryHandler(IComponentRepository componentRepository, ILogger<GetComponentsByBoardIdQueryHandler> logger)
    {
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<IEnumerable<Component>>> Handle(GetComponentsByBoardIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
