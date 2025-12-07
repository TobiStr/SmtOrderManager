using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Infrastructure.BlobStorage;
using SmtOrderManager.Infrastructure.CosmosDb;
using SmtOrderManager.Tests.Application.TestHelpers;
using System.Net;
using Xunit;

namespace SmtOrderManager.Tests.Infrastructure.CosmosDb;

public class ComponentRepositoryTests
{
    private readonly CosmosDbOptions _options = new()
    {
        DatabaseName = "TestDb",
        Containers = new CosmosDbContainers
        {
            Components = "Components"
        }
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsComponent_WhenFound()
    {
        var component = Component.Create("C1", "desc");
        var (repo, containerMock) = CreateRepository();

        containerMock
            .Setup(c => c.ReadItemAsync<Component>(
                component.Id.ToString(),
                It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey(component.Id.ToString()))),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CosmosTestHelpers.CreateItemResponse(component));

        var result = await repo.GetByIdAsync(component.Id, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(component, result.GetOk());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsError_WhenNotFound()
    {
        var componentId = Guid.NewGuid();
        var (repo, containerMock) = CreateRepository();

        containerMock
            .Setup(c => c.ReadItemAsync<Component>(
                componentId.ToString(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("not found", HttpStatusCode.NotFound, 0, string.Empty, 0));

        var result = await repo.GetByIdAsync(componentId, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddOrUpdateAsync_Upserts_Component()
    {
        var component = Component.Create("C1", "desc");
        var (repo, containerMock) = CreateRepository();

        containerMock
            .Setup(c => c.UpsertItemAsync(
                component,
                It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey(component.Id.ToString()))),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CosmosTestHelpers.CreateItemResponse(component));

        var result = await repo.AddOrUpdateAsync(component, CancellationToken.None);

        Assert.True(result.Success);
        containerMock.Verify(c => c.UpsertItemAsync(
            component,
            It.IsAny<PartitionKey>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsList_WhenFound()
    {
        var component = Component.Create("C1", "desc");
        var feedResponse = CosmosTestHelpers.CreateFeedResponse(new[] { component });
        var feedIterator = CosmosTestHelpers.CreateFeedIterator(feedResponse);

        var (repo, containerMock) = CreateRepository();

        containerMock
            .Setup(c => c.GetItemQueryIterator<Component>(
                It.IsAny<QueryDefinition>(),
                null,
                null))
            .Returns(feedIterator);

        var result = await repo.GetByIdsAsync(new[] { component.Id }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.GetOk());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsError_WhenNotFound()
    {
        var componentId = Guid.NewGuid();
        var (repo, containerMock) = CreateRepository();

        containerMock
            .Setup(c => c.ReadItemAsync<Component>(
                componentId.ToString(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("not found", HttpStatusCode.NotFound, 0, string.Empty, 0));

        var result = await repo.DeleteAsync(componentId, CancellationToken.None);

        Assert.False(result.Success);
    }

    private (ComponentRepository Repository, Mock<Container> ContainerMock) CreateRepository()
    {
        var containerMock = new Mock<Container>();
        var databaseMock = new Mock<Database>();

        databaseMock.Setup(d => d.GetContainer(_options.Containers.Components))
            .Returns(containerMock.Object);

        var clientMock = new Mock<CosmosClient>();
        clientMock.Setup(c => c.GetDatabase(_options.DatabaseName))
            .Returns(databaseMock.Object);

        var loggerFactory = TestLoggerFactory.Create();
        var logger = loggerFactory.CreateLogger<ComponentRepository>();
        var repo = new ComponentRepository(clientMock.Object, Options.Create(_options), Options.Create(new ImageUrlOptions()), logger);

        return (repo, containerMock);
    }
}
