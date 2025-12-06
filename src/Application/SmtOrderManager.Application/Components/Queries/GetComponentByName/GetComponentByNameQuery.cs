using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Components.Queries.GetComponentByName;

public record GetComponentByNameQuery(string Name) : IRequest<Result<Component>>;

public class GetComponentByNameQueryHandler : IRequestHandler<GetComponentByNameQuery, Result<Component>>
{
    private readonly IComponentRepository _componentRepository;
    private readonly ILogger<GetComponentByNameQueryHandler> _logger;

    public GetComponentByNameQueryHandler(IComponentRepository componentRepository, ILogger<GetComponentByNameQueryHandler> logger)
    {
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result<Component>> Handle(GetComponentByNameQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
