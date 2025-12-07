using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Components.Queries.GetAllComponents;

public record GetAllComponentsQuery : IRequest<Result<IEnumerable<Component>>>;

public class GetAllComponentsQueryHandler : IRequestHandler<GetAllComponentsQuery, Result<IEnumerable<Component>>>
{
    private readonly IComponentRepository _componentRepository;
    private readonly ILogger<GetAllComponentsQueryHandler> _logger;

    public GetAllComponentsQueryHandler(IComponentRepository componentRepository, ILogger<GetAllComponentsQueryHandler> logger)
    {
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<Component>>> Handle(GetAllComponentsQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetAllComponentsQuery handling");
        }

        try
        {
            var result = await _componentRepository.GetAllAsync(cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetAllComponentsQuery handled with success: {Success}", result.Success);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetAllComponentsQuery");
            return ex;
        }
    }
}
