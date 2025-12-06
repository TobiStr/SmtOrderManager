using Moq;
using SmtOrderManager.Application.Features.Users.Commands.AddOrderToUser;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Users.Commands.AddOrderToUser;

public class AddOrderToUserCommandTests
{
    [Fact]
    public async Task Handle_AppendsOrderId_AndUpsertsUser()
    {
        var user = User.Create("test@example.com", "Tester", "hash");
        var orderId = Guid.NewGuid();

        var repoMock = new Mock<IUserRepository>();
        repoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        repoMock.Setup(r => r.AddOrUpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<AddOrderToUserCommandHandler>();
        var handler = new AddOrderToUserCommandHandler(repoMock.Object, logger);

        var result = await handler.Handle(new AddOrderToUserCommand(user.Id, orderId), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.AddOrUpdateAsync(
            It.Is<User>(u => u.OrderIds.Contains(orderId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
