using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Components.Queries.GetComponentById;

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

    public async Task<Result<Component>> Handle(GetComponentByIdQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetComponentByIdQuery handling for Component ID: {ComponentId}", request.ComponentId);
        }

        try
        {
            var result = await _componentRepository.GetByIdAsync(request.ComponentId, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetComponentByIdQuery handled with success: {Success} for Component ID: {ComponentId}", result.Success, request.ComponentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetComponentByIdQuery for Component ID: {ComponentId}", request.ComponentId);
            return ex;
        }
    }
}
