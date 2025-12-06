using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Components.Queries.GetComponentById;

public record GetComponentByIdQuery(Guid ComponentId) : IRequest<Result<Component>>;

public class GetComponentByIdQueryHandler : IRequestHandler<GetComponentByIdQuery, Result<Component>>
{
    private readonly IComponentRepository _componentRepository;
    private readonly ILogger<GetComponentByIdQueryHandler> _logger;

    public GetComponentByIdQueryHandler(IComponentRepository componentRepository, ILogger<GetComponentByIdQueryHandler> logger)
    {
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<Component>> Handle(GetComponentByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
