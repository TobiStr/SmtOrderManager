using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Users.Queries.GetUserByEmail;

public record GetUserByEmailQuery(string Email) : IRequest<Result<User>>;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<User>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserByEmailQueryHandler> _logger;

    public GetUserByEmailQueryHandler(IUserRepository userRepository, ILogger<GetUserByEmailQueryHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<User>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetUserByEmailQuery handling for Email: {Email}", request.Email);
        }

        try
        {
            var result = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetUserByEmailQuery handled with success: {Success} for Email: {Email}", result.Success, request.Email);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetUserByEmailQuery for Email: {Email}", request.Email);
            return ex;
        }
    }
}
