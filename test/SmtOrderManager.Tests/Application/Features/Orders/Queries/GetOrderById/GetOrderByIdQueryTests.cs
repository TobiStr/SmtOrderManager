using Moq;
using SmtOrderManager.Application.Features.Orders.Queries.GetOrderById;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryTests
{
    [Fact]
    public async Task Handle_ReturnsOrder()
    {
        var order = Order.Create("order", DateTime.UtcNow, Guid.NewGuid());

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var logger = TestLoggerFactory.CreateLogger<GetOrderByIdQueryHandler>();
        var handler = new GetOrderByIdQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(order, result.GetOk());
        repoMock.Verify(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
