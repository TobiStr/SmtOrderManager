using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmtOrderManager.Domain.Repositories;
using DomainUser = SmtOrderManager.Domain.Entities.User;
using DomainOrder = SmtOrderManager.Domain.Entities.Order;

namespace SmtOrderManager.Infrastructure.CosmosDb;

/// <summary>
/// Cosmos DB implementation of the User repository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly Container _container;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        IOrderRepository orderRepository,
        ILogger<UserRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));

        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        var database = cosmosClient.GetDatabase(opts.DatabaseName);
        _container = database.GetContainer(opts.Containers.Users);
    }

    public async Task<Result<DomainUser>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByIdAsync started for User ID: {UserId}", id);
        }

        try
        {
            var response = await _container.ReadItemAsync<DomainUser>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            var userWithOrders = await PopulateOrdersAsync(response.Resource, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "GetByIdAsync completed successfully for User ID: {UserId} with {OrderCount} orders",
                    id,
                    userWithOrders.Orders.Count);
            }

            return userWithOrders;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User with ID {UserId} not found", id);
            return new Exception($"User with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving User with ID {UserId}", id);
            return ex;
        }
    }

    public async Task<Result<DomainUser>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByEmailAsync started for email: {Email}", email);
        }

        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.email = @email")
                .WithParameter("@email", email.ToLowerInvariant());

            var iterator = _container.GetItemQueryIterator<DomainUser>(query);
            var users = new List<DomainUser>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                users.AddRange(response);
            }

            if (users.Count == 0)
            {
                _logger.LogWarning("User with email '{Email}' not found", email);
                return new Exception($"User with email '{email}' not found");
            }

            var user = users.First();
            user = await PopulateOrdersAsync(user, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "GetByEmailAsync completed successfully for email: {Email} with {OrderCount} orders",
                    email,
                    user.Orders.Count);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving User with email '{Email}'", email);
            return ex;
        }
    }

    public async Task<Result<IEnumerable<DomainUser>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetAllAsync started");
        }

        try
        {
            var query = new QueryDefinition("SELECT * FROM c");

            var iterator = _container.GetItemQueryIterator<DomainUser>(query);
            var users = new List<DomainUser>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                users.AddRange(response);
            }

            var populatedUsers = new List<DomainUser>(users.Count);
            foreach (var user in users)
            {
                populatedUsers.Add(await PopulateOrdersAsync(user, cancellationToken));
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "GetAllAsync completed. Found {Count} users and populated orders",
                    populatedUsers.Count);
            }

            return populatedUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return ex;
        }
    }

    public async Task<Result> AddAsync(DomainUser user, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("AddAsync started for User ID: {UserId}, Email: {Email}", user.Id, user.Email);
        }

        try
        {
            var userToPersist = PrepareForPersistence(user);

            await _container.CreateItemAsync(
                userToPersist,
                new PartitionKey(userToPersist.Id.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("User created successfully with ID: {UserId}, Email: {Email}", user.Id, user.Email);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("AddAsync completed for User ID: {UserId}", user.Id);
            }

            return Result.Ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogWarning("User with ID {UserId} already exists", user.Id);
            return new Exception($"User with ID {user.Id} already exists");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating User with ID {UserId}", user.Id);
            return ex;
        }
    }

    public async Task<Result> UpdateAsync(DomainUser user, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("UpdateAsync started for User ID: {UserId}", user.Id);
        }

        try
        {
            var userToPersist = PrepareForPersistence(user);

            await _container.ReplaceItemAsync(
                userToPersist,
                userToPersist.Id.ToString(),
                new PartitionKey(userToPersist.Id.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("User updated successfully with ID: {UserId}", user.Id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("UpdateAsync completed for User ID: {UserId}", user.Id);
            }

            return Result.Ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User with ID {UserId} not found for update", user.Id);
            return new Exception($"User with ID {user.Id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating User with ID {UserId}", user.Id);
            return ex;
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("DeleteAsync started for User ID: {UserId}", id);
        }

        try
        {
            await _container.DeleteItemAsync<DomainUser>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("User deleted successfully with ID: {UserId}", id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteAsync completed for User ID: {UserId}", id);
            }

            return Result.Ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User with ID {UserId} not found for deletion", id);
            return new Exception($"User with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting User with ID {UserId}", id);
            return ex;
        }
    }

    private async Task<DomainUser> PopulateOrdersAsync(DomainUser user, CancellationToken cancellationToken)
    {
        if (user.OrderIds.Count == 0)
        {
            return user with { Orders = Array.Empty<DomainOrder>() };
        }

        var ordersResult = await _orderRepository.GetByIdsAsync(user.OrderIds, cancellationToken);
        if (ordersResult.Success)
        {
            var orders = ordersResult.GetOk();
            return user with { Orders = orders.ToList() };
        }

        _logger.LogWarning(
            "Unable to load orders for User ID: {UserId}. Reason: {Message}",
            user.Id,
            ordersResult.GetError().Message);

        return user with { Orders = Array.Empty<DomainOrder>() };
    }

    private static DomainUser PrepareForPersistence(DomainUser user)
    {
        var orderIds = new List<Guid>(user.OrderIds);

        foreach (var order in user.Orders)
        {
            if (!orderIds.Contains(order.Id))
            {
                orderIds.Add(order.Id);
            }
        }

        return user with
        {
            OrderIds = orderIds,
            Orders = Array.Empty<DomainOrder>()
        };
    }
}
