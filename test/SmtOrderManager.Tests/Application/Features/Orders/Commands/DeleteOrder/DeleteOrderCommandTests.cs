using Moq;
using SmtOrderManager.Application.Features.Orders.Commands.DeleteOrder;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandTests
{
    [Fact]
    public async Task Handle_DeletesOrder()
    {
        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<DeleteOrderCommandHandler>();
        var handler = new DeleteOrderCommandHandler(repoMock.Object, logger);

        var orderId = Guid.NewGuid();
        var result = await handler.Handle(new DeleteOrderCommand(orderId), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.DeleteAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
