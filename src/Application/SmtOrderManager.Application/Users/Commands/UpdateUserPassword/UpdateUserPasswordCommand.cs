using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Users.Commands.UpdateUserPassword;

public record UpdateUserPasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result>;

public class UpdateUserPasswordCommandHandler : IRequestHandler<UpdateUserPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<UpdateUserPasswordCommandHandler> _logger;

    public UpdateUserPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        ILogger<UpdateUserPasswordCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(UpdateUserPasswordCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("UpdateUserPasswordCommand handling for User ID: {UserId}", request.UserId);
        }

        try
        {
            var userResult = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (!userResult.Success)
            {
                return userResult.GetError();
            }

            var user = userResult.GetOk();
            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return new UnauthorizedAccessException("Current password is invalid.");
            }

            var newHash = _passwordHasher.HashPassword(user, request.NewPassword);
            var updatedUser = user with { PasswordHash = newHash, UpdatedAt = DateTime.UtcNow };

            var saveResult = await _userRepository.AddOrUpdateAsync(updatedUser, cancellationToken);
            if (!saveResult.Success)
            {
                return saveResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("UpdateUserPasswordCommand handled successfully for User ID: {UserId}", request.UserId);
            }

            return Result.Ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling UpdateUserPasswordCommand for User ID: {UserId}", request.UserId);
            return ex;
        }
    }
}
