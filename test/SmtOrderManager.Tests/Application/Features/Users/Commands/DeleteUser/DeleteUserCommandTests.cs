using Moq;
using SmtOrderManager.Application.Features.Users.Commands.DeleteUser;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandTests
{
    [Fact]
    public async Task Handle_DeletesUser()
    {
        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<DeleteUserCommandHandler>();
        var handler = new DeleteUserCommandHandler(repoMock.Object, logger);

        var userId = Guid.NewGuid();
        var result = await handler.Handle(new DeleteUserCommand(userId), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
