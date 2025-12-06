using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Components.Commands.DeleteComponent;

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

    public Task<Result> Handle(DeleteComponentCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
