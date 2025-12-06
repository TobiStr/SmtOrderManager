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

    public Task<Result<User>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
