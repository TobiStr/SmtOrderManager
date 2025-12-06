using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Infrastructure.CosmosDb;

/// <summary>
/// Cosmos DB implementation of the Board repository.
/// </summary>
public class BoardRepository : IBoardRepository
{
    private readonly Container _container;
    private readonly IComponentRepository _componentRepository;
    private readonly ILogger<BoardRepository> _logger;

    public BoardRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        IComponentRepository componentRepository,
        ILogger<BoardRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));

        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        var database = cosmosClient.GetDatabase(opts.DatabaseName);
        _container = database.GetContainer(opts.Containers.Boards);
    }

    public async Task<Result<Board>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByIdAsync started for Board ID: {BoardId}", id);
        }

        try
        {
            var response = await _container.ReadItemAsync<Board>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            var board = response.Resource;

            // Load components by IDs
            if (board.ComponentIds.Any())
            {
                var componentsResult = await _componentRepository.GetByIdsAsync(board.ComponentIds, cancellationToken);
                if (componentsResult.Success)
                {
                    var components = componentsResult.GetOk();
                    board = board with { Components = components.ToList() };
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetByIdAsync completed successfully for Board ID: {BoardId} with {ComponentCount} components",
                    id, board.Components.Count);
            }

            return board;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Board with ID {BoardId} not found", id);
            return new Exception($"Board with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Board with ID {BoardId}", id);
            return ex;
        }
    }

    public async Task<Result<IEnumerable<Board>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByIdsAsync started for {Count} board IDs", ids.Count());
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
                return Result<IEnumerable<Board>>.Ok(Enumerable.Empty<Board>());
            }

            var boards = new List<Board>();

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

                var iterator = _container.GetItemQueryIterator<Board>(queryDef);
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    boards.AddRange(response);
                }
            }

            // Load components for each board using their ComponentIds
            var populatedBoards = new List<Board>();
            foreach (var board in boards)
            {
                if (board.ComponentIds.Any())
                {
                    var componentsResult = await _componentRepository.GetByIdsAsync(board.ComponentIds, cancellationToken);
                    if (componentsResult.Success)
                    {
                        var components = componentsResult.GetOk();
                        populatedBoards.Add(board with { Components = components.ToList() });
                    }
                    else
                    {
                        populatedBoards.Add(board);
                    }
                }
                else
                {
                    populatedBoards.Add(board);
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetByIdsAsync completed. Found {Count} boards", populatedBoards.Count);
            }

            return populatedBoards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving boards by IDs");
            return ex;
        }
    }

    public async Task<Result<Board>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByNameAsync started for Board name: {BoardName}", name);
        }

        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.name = @name")
                .WithParameter("@name", name);

            var iterator = _container.GetItemQueryIterator<Board>(query);
            var boards = new List<Board>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                boards.AddRange(response);
            }

            if (boards.Count == 0)
            {
                _logger.LogWarning("Board with name '{BoardName}' not found", name);
                return new Exception($"Board with name '{name}' not found");
            }

            var board = boards.First();

            // Load components by IDs
            if (board.ComponentIds.Any())
            {
                var componentsResult = await _componentRepository.GetByIdsAsync(board.ComponentIds, cancellationToken);
                if (componentsResult.Success)
                {
                    var components = componentsResult.GetOk();
                    board = board with { Components = components.ToList() };
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetByNameAsync completed successfully for Board name: {BoardName} with {ComponentCount} components",
                    name, board.Components.Count);
            }

            return board;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Board with name '{BoardName}'", name);
            return ex;
        }
    }

    public async Task<Result<IEnumerable<Board>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetAllAsync started for boards");
        }

        try
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<Board>(query);
            var boards = new List<Board>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                boards.AddRange(response);
            }

            var populatedBoards = new List<Board>();
            foreach (var board in boards)
            {
                if (board.ComponentIds.Any())
                {
                    var componentsResult = await _componentRepository.GetByIdsAsync(board.ComponentIds, cancellationToken);
                    if (componentsResult.Success)
                    {
                        var components = componentsResult.GetOk();
                        populatedBoards.Add(board with { Components = components.ToList() });
                    }
                    else
                    {
                        populatedBoards.Add(board);
                    }
                }
                else
                {
                    populatedBoards.Add(board);
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetAllAsync completed. Found {Count} boards", populatedBoards.Count);
            }

            return populatedBoards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all boards");
            return ex;
        }
    }

    public async Task<Result> AddOrUpdateAsync(Board board, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("AddOrUpdateAsync started for Board ID: {BoardId}", board.Id);
        }

        try
        {
            var boardToPersist = PrepareForPersistence(board);

            await _container.UpsertItemAsync(
                boardToPersist,
                new PartitionKey(boardToPersist.OrderId.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Board upserted successfully with ID: {BoardId} for Order ID: {OrderId}",
                board.Id,
                board.OrderId);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("AddOrUpdateAsync completed for Board ID: {BoardId}", board.Id);
            }

            return Result.Ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting Board with ID {BoardId}", board.Id);
            return ex;
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("DeleteAsync started for Board ID: {BoardId}", id);
        }

        try
        {
            // First, get the board to retrieve the partition key (orderId)
            var boardResult = await GetByIdAsync(id, cancellationToken);
            if (!boardResult.Success)
            {
                return boardResult.GetError();
            }

            var board = boardResult.GetOk();

            await _container.DeleteItemAsync<Board>(
                id.ToString(),
                new PartitionKey(board.OrderId.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Board deleted successfully with ID: {BoardId}", id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteAsync completed for Board ID: {BoardId}", id);
            }

            return Result.Ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Board with ID {BoardId} not found for deletion", id);
            return new Exception($"Board with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Board with ID {BoardId}", id);
            return ex;
        }
    }

    private static Board PrepareForPersistence(Board board)
    {
        var componentIds = new List<Guid>(board.ComponentIds);

        foreach (var component in board.Components)
        {
            if (!componentIds.Contains(component.Id))
            {
                componentIds.Add(component.Id);
            }
        }

        return board with
        {
            ComponentIds = componentIds,
            Components = Array.Empty<Component>()
        };
    }
}
