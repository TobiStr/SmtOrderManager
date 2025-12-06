using Moq;
using SmtOrderManager.Application.Features.Orders.Queries.SearchOrders;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Orders.Queries.SearchOrders;

public class SearchOrdersQueryTests
{
    [Fact]
    public async Task Handle_UsesRepository()
    {
        var orders = new[] { Order.Create("order", DateTime.UtcNow, Guid.NewGuid()) };

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Order>>.Ok(orders));

        var logger = TestLoggerFactory.CreateLogger<SearchOrdersQueryHandler>();
        var handler = new SearchOrdersQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new SearchOrdersQuery("order"), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
