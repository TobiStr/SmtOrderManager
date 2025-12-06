using MediatR;
using Microsoft.Extensions.Logging;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Application.Users.Commands.UpdateUserPassword;

public record UpdateUserPasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result>;

public class UpdateUserPasswordCommandHandler : IRequestHandler<UpdateUserPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserPasswordCommandHandler> _logger;

    public UpdateUserPasswordCommandHandler(IUserRepository userRepository, ILogger<UpdateUserPasswordCommandHandler> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<Result> Handle(UpdateUserPasswordCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
