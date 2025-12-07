using Moq;
using Microsoft.AspNetCore.Identity;
using SmtOrderManager.Application.Features.Users.Commands.LoginUser;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Users.Commands.LoginUser;

public class LoginUserCommandTests
{
    [Fact]
    public async Task Handle_VerifiesPassword_AndReturnsUser()
    {
        var user = User.Create("test@example.com", "Tester", "hash");
        var hasher = new PasswordHasher<User>();
        var hashed = hasher.HashPassword(user, "secret");
        user = user with { PasswordHash = hashed };

        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var logger = TestLoggerFactory.CreateLogger<LoginUserCommandHandler>();
        var handler = new LoginUserCommandHandler(repoMock.Object, hasher, logger);

        var result = await handler.Handle(new LoginUserCommand(user.Email, "secret"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(user.Email, result.GetOk().Email);
        repoMock.Verify(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()), Times.Once);
    }
}
