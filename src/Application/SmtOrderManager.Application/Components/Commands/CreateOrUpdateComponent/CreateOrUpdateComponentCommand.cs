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

    public Task<Result<Component>> Handle(CreateOrUpdateComponentCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
