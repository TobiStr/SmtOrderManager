using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Features.Users.Commands.AddOrderToUser;

public record AddOrderToUserCommand(Guid UserId, Guid OrderId) : IRequest<Result<User>>;

public class AddOrderToUserCommandHandler : IRequestHandler<AddOrderToUserCommand, Result<User>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AddOrderToUserCommandHandler> _logger;

    public AddOrderToUserCommandHandler(IUserRepository userRepository, ILogger<AddOrderToUserCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<User>> Handle(AddOrderToUserCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("AddOrderToUserCommand handling for User ID: {UserId}, Order ID: {OrderId}", request.UserId, request.OrderId);
        }

        try
        {
            var userResult = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (!userResult.Success)
            {
                return userResult.GetError();
            }

            var user = userResult.GetOk();
            var updatedUser = user.OrderIds.Contains(request.OrderId)
                ? user
                : user with { OrderIds = user.OrderIds.Concat(new[] { request.OrderId }).ToList() };

            var saveResult = await _userRepository.AddOrUpdateAsync(updatedUser, cancellationToken);
            if (!saveResult.Success)
            {
                return saveResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("AddOrderToUserCommand handled successfully for User ID: {UserId}", request.UserId);
            }

            return updatedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling AddOrderToUserCommand for User ID: {UserId}", request.UserId);
            return ex;
        }
    }
}
