using Moq;
using SmtOrderManager.Application.Features.Orders.Commands.CreateOrUpdateOrder;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Orders.Commands.CreateOrUpdateOrder;

public class CreateOrUpdateOrderCommandTests
{
    [Fact]
    public async Task Handle_UpsertsOrder()
    {
        var order = Order.Create("order", DateTime.UtcNow, Guid.NewGuid());

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.AddOrUpdateAsync(order, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<CreateOrUpdateOrderCommandHandler>();
        var handler = new CreateOrUpdateOrderCommandHandler(repoMock.Object, logger);

        var result = await handler.Handle(new CreateOrUpdateOrderCommand(order), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.AddOrUpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }
}
