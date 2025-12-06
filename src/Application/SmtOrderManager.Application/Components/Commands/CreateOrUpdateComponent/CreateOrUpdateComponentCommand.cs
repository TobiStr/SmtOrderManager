using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Components.Commands.CreateOrUpdateComponent;

public record CreateOrUpdateComponentCommand(Component Component) : IRequest<Result<Component>>;

public class CreateOrUpdateComponentCommandHandler : IRequestHandler<CreateOrUpdateComponentCommand, Result<Component>>
{
    private readonly IComponentRepository _componentRepository;
    private readonly ILogger<CreateOrUpdateComponentCommandHandler> _logger;

    public CreateOrUpdateComponentCommandHandler(IComponentRepository componentRepository, ILogger<CreateOrUpdateComponentCommandHandler> logger)
    {
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Component>> Handle(CreateOrUpdateComponentCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("CreateOrUpdateComponentCommand handling for Component ID: {ComponentId}", request.Component.Id);
        }

        try
        {
            var upsertResult = await _componentRepository.AddOrUpdateAsync(request.Component, cancellationToken);
            if (!upsertResult.Success)
            {
                return upsertResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("CreateOrUpdateComponentCommand handled successfully for Component ID: {ComponentId}", request.Component.Id);
            }

            return request.Component;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CreateOrUpdateComponentCommand for Component ID: {ComponentId}", request.Component.Id);
            return ex;
        }
    }
}
