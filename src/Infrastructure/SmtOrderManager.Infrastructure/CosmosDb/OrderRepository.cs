using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Infrastructure.CosmosDb;

/// <summary>
/// Cosmos DB implementation of the Order repository.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly Container _container;
    private readonly IBoardRepository _boardRepository;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        IBoardRepository boardRepository,
        ILogger<OrderRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _boardRepository = boardRepository ?? throw new ArgumentNullException(nameof(boardRepository));

        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        var database = cosmosClient.GetDatabase(opts.DatabaseName);
        _container = database.GetContainer(opts.Containers.Orders);
    }

    public async Task<Result<Order>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByIdAsync started for Order ID: {OrderId}", id);
        }

        try
        {
            var response = await _container.ReadItemAsync<Order>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            var order = response.Resource;

            // Load boards by IDs (boards will load their components)
            if (order.BoardIds.Any())
            {
                var boardIds = order.BoardIds.Select(b => b.Id);
                var boardsResult = await _boardRepository.GetByIdsAsync(boardIds, cancellationToken);
                if (boardsResult.Success)
                {
                    var boards = boardsResult.GetOk();
                    order = order with { Boards = boards.ToList() };
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetByIdAsync completed successfully for Order ID: {OrderId} with {BoardCount} boards",
                    id, order.Boards.Count);
            }

            return order;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", id);
            return new Exception($"Order with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Order with ID {OrderId}", id);
            return ex;
        }
    }

    public async Task<Result<IEnumerable<Order>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByIdsAsync started for {Count} order IDs", ids.Count());
        }

        try
        {
            var idList = ids.ToList();
            if (!idList.Any())
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("GetByIdsAsync completed with empty ID list");
                }
                return Result<IEnumerable<Order>>.Ok(Enumerable.Empty<Order>());
            }

            var orders = new List<Order>();

            // Cosmos DB has a limit on IN clause, so we batch the queries
            const int batchSize = 100;
            for (int i = 0; i < idList.Count; i += batchSize)
            {
                var batch = idList.Skip(i).Take(batchSize).ToList();
                var idParams = string.Join(",", batch.Select((_, index) => $"@id{index}"));
                var queryText = $"SELECT * FROM c WHERE c.id IN ({idParams})";

                var queryDef = new QueryDefinition(queryText);
                for (int j = 0; j < batch.Count; j++)
                {
                    queryDef = queryDef.WithParameter($"@id{j}", batch[j].ToString());
                }

                var iterator = _container.GetItemQueryIterator<Order>(queryDef);
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    orders.AddRange(response);
                }
            }

            // Load boards with components for each order using their BoardIds
            var populatedOrders = new List<Order>();
            foreach (var order in orders)
            {
                if (order.BoardIds.Any())
                {
                    var boardIds = order.BoardIds.Select(b => b.Id);
                    var boardsResult = await _boardRepository.GetByIdsAsync(boardIds, cancellationToken);
                    if (boardsResult.Success)
                    {
                        var boards = boardsResult.GetOk();
                        populatedOrders.Add(order with { Boards = boards.ToList() });
                    }
                    else
                    {
                        populatedOrders.Add(order);
                    }
                }
                else
                {
                    populatedOrders.Add(order);
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetByIdsAsync completed. Found {Count} orders", populatedOrders.Count);
            }

            return populatedOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders by IDs");
            return ex;
        }
    }

    public async Task<Result> AddOrUpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("AddOrUpdateAsync started for Order ID: {OrderId}", order.Id);
        }

        try
        {
            var orderToPersist = PrepareForPersistence(order);

            await _container.UpsertItemAsync(
                orderToPersist,
                new PartitionKey(orderToPersist.Id.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Order upserted successfully with ID: {OrderId}", order.Id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("AddOrUpdateAsync completed for Order ID: {OrderId}", order.Id);
            }

            return Result.Ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Order with ID {OrderId} not found for update", order.Id);
            return new Exception($"Order with ID {order.Id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Order with ID {OrderId}", order.Id);
            return ex;
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("DeleteAsync started for Order ID: {OrderId}", id);
        }

        try
        {
            await _container.DeleteItemAsync<Order>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Order deleted successfully with ID: {OrderId}", id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteAsync completed for Order ID: {OrderId}", id);
            }

            return Result.Ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Order with ID {OrderId} not found for deletion", id);
            return new Exception($"Order with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Order with ID {OrderId}", id);
            return ex;
        }
    }

    private static Order PrepareForPersistence(Order order)
    {
        return order with
        {
            Boards = Array.Empty<Board>()
        };
    }
}
