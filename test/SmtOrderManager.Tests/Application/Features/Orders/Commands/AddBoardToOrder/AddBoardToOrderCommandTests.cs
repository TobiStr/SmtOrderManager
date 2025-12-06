using Moq;
using SmtOrderManager.Application.Features.Orders.Commands.AddBoardToOrder;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Orders.Commands.AddBoardToOrder;

public class AddBoardToOrderCommandTests
{
    [Fact]
    public async Task Handle_AppendsBoardId_AndUpsertsOrder()
    {
        var order = Order.Create("order", DateTime.UtcNow, Guid.NewGuid());
        var boardId = Guid.NewGuid();

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        repoMock.Setup(r => r.AddOrUpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<AddBoardToOrderCommandHandler>();
        var handler = new AddBoardToOrderCommandHandler(repoMock.Object, logger);

        var result = await handler.Handle(new AddBoardToOrderCommand(order.Id, boardId), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.AddOrUpdateAsync(
            It.Is<Order>(o => o.BoardIds.Contains(boardId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
