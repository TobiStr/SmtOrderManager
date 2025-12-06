using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest<Result>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(IUserRepository userRepository, ILogger<DeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("DeleteUserCommand handling for User ID: {UserId}", request.UserId);
        }

        try
        {
            var deleteResult = await _userRepository.DeleteAsync(request.UserId, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteUserCommand handled with success: {Success} for User ID: {UserId}", deleteResult.Success, request.UserId);
            }

            return deleteResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling DeleteUserCommand for User ID: {UserId}", request.UserId);
            return ex;
        }
    }
}
