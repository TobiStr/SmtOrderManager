using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Domain.Services;

namespace SmtOrderManager.Application.Features.Components.Commands.CreateOrUpdateComponent;

public record CreateOrUpdateComponentCommand(Component Component, Stream? ImageStream = null, string? ImageFileName = null) : IRequest<Result<Component>>;

public class CreateOrUpdateComponentCommandHandler : IRequestHandler<CreateOrUpdateComponentCommand, Result<Component>>
{
    private readonly IComponentRepository _componentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<CreateOrUpdateComponentCommandHandler> _logger;

    public CreateOrUpdateComponentCommandHandler(
        IComponentRepository componentRepository,
        IBlobStorageService blobStorageService,
        ILogger<CreateOrUpdateComponentCommandHandler> logger)
    {
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
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
            var componentToSave = request.Component;

            if (request.ImageStream is not null && !string.IsNullOrWhiteSpace(request.ImageFileName))
            {
                var blobName = $"{componentToSave.Id}/{request.ImageFileName}";
                var uploadResult = await _blobStorageService.UploadAsync(blobName, request.ImageStream, cancellationToken);
                if (!uploadResult.Success)
                {
                    return uploadResult.GetError();
                }

                var imageUrl = _blobStorageService.GetBlobUrl(blobName);
                componentToSave = componentToSave with { ImageUrl = imageUrl };
            }

            var upsertResult = await _componentRepository.AddOrUpdateAsync(componentToSave, cancellationToken);
            if (!upsertResult.Success)
            {
                return upsertResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("CreateOrUpdateComponentCommand handled successfully for Component ID: {ComponentId}", componentToSave.Id);
            }

            return componentToSave;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CreateOrUpdateComponentCommand for Component ID: {ComponentId}", request.Component.Id);
            return ex;
        }
    }
}
