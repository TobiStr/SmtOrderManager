using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Infrastructure.CosmosDb;
using SmtOrderManager.Tests.Application.TestHelpers;
using System.Net;
using Xunit;

namespace SmtOrderManager.Tests.Infrastructure.CosmosDb;

public class OrderRepositoryTests
{
    private readonly CosmosDbOptions _options = new()
    {
        DatabaseName = "TestDb",
        Containers = new CosmosDbContainers
        {
            Orders = "Orders"
        }
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsOrder_WithBoards()
    {
        var order = Order.Create("order", DateTime.UtcNow, Guid.NewGuid()) with
        {
            BoardIds = new List<Guid> { Guid.NewGuid() }
        };
        var board = Board.Create("B1", "desc", 10, 5);

        var boardRepoMock = new Mock<IBoardRepository>();
        boardRepoMock.Setup(r => r.GetByIdsAsync(order.BoardIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Board>>.Ok(new[] { board }));

        var (repo, containerMock) = CreateRepository(boardRepoMock.Object);

        containerMock
            .Setup(c => c.ReadItemAsync<Order>(
                order.Id.ToString(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CosmosTestHelpers.CreateItemResponse(order));

        var result = await repo.GetByIdAsync(order.Id, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.GetOk().Boards);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpsertsOrder()
    {
        var order = Order.Create("order", DateTime.UtcNow, Guid.NewGuid());
        var board = Board.Create("B1", "desc", 10, 5);
        order = order with { Boards = new List<Board> { board } };

        var boardRepoMock = new Mock<IBoardRepository>();

        var (repo, containerMock) = CreateRepository(boardRepoMock.Object);

        containerMock
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<Order>(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CosmosTestHelpers.CreateItemResponse(order));

        var result = await repo.AddOrUpdateAsync(order, CancellationToken.None);

        Assert.True(result.Success);
        containerMock.Verify(c => c.UpsertItemAsync(
            It.Is<Order>(o => o.Id == order.Id),
            It.IsAny<PartitionKey>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsError_OnException()
    {
        var boardRepoMock = new Mock<IBoardRepository>();
        var (repo, containerMock) = CreateRepository(boardRepoMock.Object);

        var iteratorMock = new Mock<FeedIterator<Order>>();
        iteratorMock.SetupSequence(i => i.HasMoreResults).Returns(true).Returns(false);
        iteratorMock.Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("network", HttpStatusCode.RequestTimeout, 0, string.Empty, 0));

        containerMock.Setup(c => c.GetItemQueryIterator<Order>(
            It.IsAny<QueryDefinition>(),
            null,
            null)).Returns(iteratorMock.Object);

        var result = await repo.GetByIdsAsync(new[] { Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.Success);
    }

    private (OrderRepository Repository, Mock<Container> ContainerMock) CreateRepository(IBoardRepository boardRepository)
    {
        var containerMock = new Mock<Container>();
        var databaseMock = new Mock<Database>();

        databaseMock.Setup(d => d.GetContainer(_options.Containers.Orders))
            .Returns(containerMock.Object);

        var clientMock = new Mock<CosmosClient>();
        clientMock.Setup(c => c.GetDatabase(_options.DatabaseName))
            .Returns(databaseMock.Object);

        var loggerFactory = TestLoggerFactory.Create();
        ILogger<OrderRepository> logger = loggerFactory.CreateLogger<OrderRepository>();
        var repo = new OrderRepository(clientMock.Object, Options.Create(_options), boardRepository, logger);

        return (repo, containerMock);
    }
}
