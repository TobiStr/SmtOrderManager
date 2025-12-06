using Moq;
using SmtOrderManager.Application.Features.Users.Queries.GetUserByEmail;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Users.Queries.GetUserByEmail;

public class GetUserByEmailQueryTests
{
    [Fact]
    public async Task Handle_ReturnsUser()
    {
        var user = User.Create("test@example.com", "Tester", "hash");

        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var logger = TestLoggerFactory.CreateLogger<GetUserByEmailQueryHandler>();
        var handler = new GetUserByEmailQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetUserByEmailQuery(user.Email), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(user.Email, result.GetOk().Email);
        repoMock.Verify(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()), Times.Once);
    }
}
