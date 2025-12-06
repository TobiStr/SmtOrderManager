using Moq;
using Microsoft.AspNetCore.Identity;
using SmtOrderManager.Application.Features.Users.Commands.RegisterUser;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandTests
{
    [Fact]
    public async Task Handle_CreatesUser_WhenEmailNotTaken()
    {
        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Exception("not found"));
        repoMock.Setup(r => r.AddOrUpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var hasher = new PasswordHasher<User>();
        var logger = TestLoggerFactory.CreateLogger<RegisterUserCommandHandler>();
        var handler = new RegisterUserCommandHandler(repoMock.Object, hasher, logger);

        var result = await handler.Handle(new RegisterUserCommand("test@example.com", "Tester", "secret"), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.AddOrUpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
