using Moq;
using SmtOrderManager.Application.Features.Users.Queries.GetAllUsers;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryTests
{
    [Fact]
    public async Task Handle_ReturnsUsers()
    {
        var users = new[] { User.Create("test@example.com", "Tester", "hash") };

        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<User>>.Ok(users));

        var logger = TestLoggerFactory.CreateLogger<GetAllUsersQueryHandler>();
        var handler = new GetAllUsersQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
