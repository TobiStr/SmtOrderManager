using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Infrastructure.CosmosDb;
using Xunit;
using DomainUser = SmtOrderManager.Domain.Entities.User;

namespace SmtOrderManager.Tests.Infrastructure.CosmosDb;

public class UserRepositoryTests
{
    private readonly CosmosDbOptions _options = new()
    {
        DatabaseName = "TestDb",
        Containers = new CosmosDbContainers
        {
            Users = "Users"
        }
    };

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WithOrders()
    {
        var orderId = Guid.NewGuid();
        var user = DomainUser.Create("user@example.com", "Test", "hash") with
        {
            OrderIds = new List<Guid> { orderId }
        };
        var order = Order.Create("order", DateTime.UtcNow, user.Id);

        var orderRepoMock = new Mock<IOrderRepository>();
        orderRepoMock.Setup(r => r.GetByIdsAsync(user.OrderIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Order>>.Ok(new[] { order }));

        var (repo, containerMock) = CreateRepository(orderRepoMock.Object);

        containerMock
            .Setup(c => c.ReadItemAsync<DomainUser>(
                user.Id.ToString(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CosmosTestHelpers.CreateItemResponse(user));

        var result = await repo.GetByIdAsync(user.Id, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.GetOk().Orders);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpsertsUser()
    {
        var user = DomainUser.Create("user@example.com", "Test", "hash");
        var order = Order.Create("order", DateTime.UtcNow, user.Id);
        user = user with { Orders = new List<Order> { order } };

        var orderRepoMock = new Mock<IOrderRepository>();

        var (repo, containerMock) = CreateRepository(orderRepoMock.Object);

        containerMock
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<DomainUser>(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CosmosTestHelpers.CreateItemResponse(user));

        var result = await repo.AddOrUpdateAsync(user, CancellationToken.None);

        Assert.True(result.Success);
        containerMock.Verify(c => c.UpsertItemAsync(
            It.Is<DomainUser>(u => u.Id == user.Id),
            It.IsAny<PartitionKey>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsError_WhenMissing()
    {
        var orderRepoMock = new Mock<IOrderRepository>();
        var (repo, containerMock) = CreateRepository(orderRepoMock.Object);

        var emptyIterator = CosmosTestHelpers.CreateFeedIterator<DomainUser>();
        containerMock.Setup(c => c.GetItemQueryIterator<DomainUser>(
            It.IsAny<QueryDefinition>(),
            null,
            null)).Returns(emptyIterator);

        var result = await repo.GetByEmailAsync("missing@example.com", CancellationToken.None);

        Assert.False(result.Success);
    }

    private (UserRepository Repository, Mock<Container> ContainerMock) CreateRepository(IOrderRepository orderRepository)
    {
        var containerMock = new Mock<Container>();
        var databaseMock = new Mock<Database>();

        databaseMock.Setup(d => d.GetContainer(_options.Containers.Users))
            .Returns(containerMock.Object);

        var clientMock = new Mock<CosmosClient>();
        clientMock.Setup(c => c.GetDatabase(_options.DatabaseName))
            .Returns(databaseMock.Object);

        var loggerFactory = CosmosTestHelpers.CreateLoggerFactory();
        ILogger<UserRepository> logger = loggerFactory.CreateLogger<UserRepository>();
        var repo = new UserRepository(clientMock.Object, Options.Create(_options), orderRepository, logger);

        return (repo, containerMock);
    }
}
