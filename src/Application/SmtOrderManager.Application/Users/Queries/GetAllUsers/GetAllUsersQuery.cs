using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<IEnumerable<User>>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<User>>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(IUserRepository userRepository, ILogger<GetAllUsersQueryHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<User>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetAllUsersQuery handling");
        }

        try
        {
            var result = await _userRepository.GetAllAsync(cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetAllUsersQuery handled with success: {Success}", result.Success);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetAllUsersQuery");
            return ex;
        }
    }
}
