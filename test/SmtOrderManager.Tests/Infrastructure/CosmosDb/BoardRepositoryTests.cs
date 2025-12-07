using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Primitives;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Infrastructure.CosmosDb;
using SmtOrderManager.Tests.Application.TestHelpers;
using System.Net;
using Xunit;

namespace SmtOrderManager.Tests.Infrastructure.CosmosDb;

public class BoardRepositoryTests
{
    private readonly CosmosDbOptions _options = new()
    {
        DatabaseName = "TestDb",
        Containers = new CosmosDbContainers
        {
            Boards = "Boards"
        }
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsBoard_WithComponents()
    {
        var componentId = Guid.NewGuid();
        var board = Board.Create("B1", "desc", 10, 5) with
        {
            ComponentIds = new List<QuantizedId> { new(componentId, 5) }
        };
        var component = Component.Create("C1", "desc");

        var componentRepoMock = new Mock<IComponentRepository>();
        componentRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Component>>.Ok(new[] { component }));

        var (repo, containerMock) = CreateRepository(componentRepoMock.Object);

        containerMock
            .Setup(c => c.ReadItemAsync<Board>(
                board.Id.ToString(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CosmosTestHelpers.CreateItemResponse(board));

        var result = await repo.GetByIdAsync(board.Id, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.GetOk().Components);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpsertsBoard()
    {
        var board = Board.Create("B1", "desc", 10, 5);
        var component = Component.Create("C1", "desc");
        board = board with { Components = new List<Component> { component } };

        var componentRepoMock = new Mock<IComponentRepository>();

        var (repo, containerMock) = CreateRepository(componentRepoMock.Object);

        containerMock
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<Board>(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CosmosTestHelpers.CreateItemResponse(board));

        var result = await repo.AddOrUpdateAsync(board, CancellationToken.None);

        Assert.True(result.Success);
        containerMock.Verify(c => c.UpsertItemAsync(
            It.Is<Board>(b => b.Id == board.Id),
            It.IsAny<PartitionKey>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsError_WhenNotFound()
    {
        var componentRepoMock = new Mock<IComponentRepository>();
        var (repo, containerMock) = CreateRepository(componentRepoMock.Object);

        var feedIterator = CosmosTestHelpers.CreateFeedIterator<Board>();
        containerMock.Setup(c => c.GetItemQueryIterator<Board>(
            It.IsAny<QueryDefinition>(),
            null,
            null)).Returns(feedIterator);

        var result = await repo.GetByNameAsync("missing", CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsError_WhenBoardMissing()
    {
        var componentRepoMock = new Mock<IComponentRepository>();
        var (repo, containerMock) = CreateRepository(componentRepoMock.Object);
        var boardId = Guid.NewGuid();

        containerMock
            .Setup(c => c.ReadItemAsync<Board>(
                boardId.ToString(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("not found", HttpStatusCode.NotFound, 0, string.Empty, 0));

        var result = await repo.DeleteAsync(boardId, CancellationToken.None);

        Assert.False(result.Success);
    }

    private (BoardRepository Repository, Mock<Container> ContainerMock) CreateRepository(IComponentRepository componentRepository)
    {
        var containerMock = new Mock<Container>();
        var databaseMock = new Mock<Database>();

        databaseMock.Setup(d => d.GetContainer(_options.Containers.Boards))
            .Returns(containerMock.Object);

        var clientMock = new Mock<CosmosClient>();
        clientMock.Setup(c => c.GetDatabase(_options.DatabaseName))
            .Returns(databaseMock.Object);

        var loggerFactory = TestLoggerFactory.Create();
        ILogger<BoardRepository> logger = loggerFactory.CreateLogger<BoardRepository>();
        var repo = new BoardRepository(clientMock.Object, Options.Create(_options), componentRepository, logger);

        return (repo, containerMock);
    }
}
