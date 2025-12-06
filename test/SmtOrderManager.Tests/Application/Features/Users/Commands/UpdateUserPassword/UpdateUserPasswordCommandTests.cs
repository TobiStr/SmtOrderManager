using Moq;
using Microsoft.AspNetCore.Identity;
using SmtOrderManager.Application.Features.Users.Commands.UpdateUserPassword;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Users.Commands.UpdateUserPassword;

public class UpdateUserPasswordCommandTests
{
    [Fact]
    public async Task Handle_UpdatesPassword_WhenCurrentMatches()
    {
        var user = User.Create("test@example.com", "Tester", "hash");
        var hasher = new PasswordHasher<User>();
        var hashed = hasher.HashPassword(user, "oldpass");
        user = user with { PasswordHash = hashed };

        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        repoMock.Setup(r => r.AddOrUpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<UpdateUserPasswordCommandHandler>();
        var handler = new UpdateUserPasswordCommandHandler(repoMock.Object, hasher, logger);

        var result = await handler.Handle(new UpdateUserPasswordCommand(user.Id, "oldpass", "newpass"), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.AddOrUpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
