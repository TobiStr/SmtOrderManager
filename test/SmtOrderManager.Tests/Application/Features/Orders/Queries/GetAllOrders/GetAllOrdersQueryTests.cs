using Moq;
using SmtOrderManager.Application.Features.Orders.Queries.GetAllOrders;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Orders.Queries.GetAllOrders;

public class GetAllOrdersQueryTests
{
    [Fact]
    public async Task Handle_ReturnsOrders()
    {
        var orders = new[] { Order.Create("order", DateTime.UtcNow, Guid.NewGuid()) };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Order>>.Ok(orders));

        var logger = TestLoggerFactory.CreateLogger<GetAllOrdersQueryHandler>();
        var handler = new GetAllOrdersQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.GetOk());
        repoMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
