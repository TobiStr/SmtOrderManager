using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Components.Commands.DeleteComponent;

public record DeleteComponentCommand(Guid ComponentId) : IRequest<Result>;

public class DeleteComponentCommandHandler : IRequestHandler<DeleteComponentCommand, Result>
{
    private readonly IComponentRepository _componentRepository;
    private readonly ILogger<DeleteComponentCommandHandler> _logger;

    public DeleteComponentCommandHandler(IComponentRepository componentRepository, ILogger<DeleteComponentCommandHandler> logger)
    {
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(DeleteComponentCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("DeleteComponentCommand handling for Component ID: {ComponentId}", request.ComponentId);
        }

        try
        {
            var deleteResult = await _componentRepository.DeleteAsync(request.ComponentId, cancellationToken);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteComponentCommand handled with success: {Success} for Component ID: {ComponentId}", deleteResult.Success, request.ComponentId);
            }

            return deleteResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling DeleteComponentCommand for Component ID: {ComponentId}", request.ComponentId);
            return ex;
        }
    }
}
