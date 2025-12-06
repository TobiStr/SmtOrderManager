using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Name, string Password) : IRequest<Result<User>>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<User>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("RegisterUserCommand handling for Email: {Email}", request.Email);
        }

        try
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existing.Success)
            {
                return new InvalidOperationException($"User with email '{request.Email}' already exists.");
            }

            var user = User.Create(request.Email, request.Name, "");
            var hashed = _passwordHasher.HashPassword(user, request.Password);
            user = user with { PasswordHash = hashed };
            var addResult = await _userRepository.AddOrUpdateAsync(user, cancellationToken);
            if (!addResult.Success)
            {
                return addResult.GetError();
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("RegisterUserCommand handled successfully for Email: {Email}", request.Email);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling RegisterUserCommand for Email: {Email}", request.Email);
            return ex;
        }
    }
}
