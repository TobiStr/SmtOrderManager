using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<User>>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<User>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        ILogger<LoginUserCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<User>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("LoginUserCommand handling for Email: {Email}", request.Email);
        }

        try
        {
            var userResult = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (!userResult.Success)
            {
                return userResult.GetError();
            }

            var user = userResult.GetOk();

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return new UnauthorizedAccessException("Invalid credentials.");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("LoginUserCommand handled successfully for Email: {Email}", request.Email);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling LoginUserCommand for Email: {Email}", request.Email);
            return ex;
        }
    }
}
