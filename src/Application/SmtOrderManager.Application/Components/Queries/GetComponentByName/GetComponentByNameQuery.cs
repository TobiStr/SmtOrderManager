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

    public async Task<Result<Component>> Handle(GetComponentByNameQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetComponentByNameQuery handling for Component name: {ComponentName}", request.Name);
        }

        try
        {
            var result = await _componentRepository.GetByNameAsync(request.Name, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetComponentByNameQuery handled with success: {Success} for Component name: {ComponentName}", result.Success, request.Name);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetComponentByNameQuery for Component name: {ComponentName}", request.Name);
            return ex;
        }
    }
}
