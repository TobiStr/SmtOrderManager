using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Infrastructure.BlobStorage;

namespace SmtOrderManager.Infrastructure.CosmosDb;

/// <summary>
/// Cosmos DB implementation of the Component repository.
/// </summary>
public class ComponentRepository : IComponentRepository
{
    private readonly Container _container;
    private readonly ILogger<ComponentRepository> _logger;
    private readonly string? _sasToken;

    public ComponentRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        IOptions<ImageUrlOptions> imageUrlOptions,
        ILogger<ComponentRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _sasToken = imageUrlOptions?.Value?.SasToken;
        var database = cosmosClient.GetDatabase(opts.DatabaseName);
        _container = database.GetContainer(opts.Containers.Components);
    }

    public async Task<Result<Component>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByIdAsync started for Component ID: {ComponentId}", id);
        }

        try
        {
            var response = await _container.ReadItemAsync<Component>(
                id.ToString(),
                new PartitionKey(id.ToString()),
                cancellationToken: cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetByIdAsync completed successfully for Component ID: {ComponentId}", id);
            }

            return AppendSasToken(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Component with ID {ComponentId} not found", id);
            return new Exception($"Component with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Component with ID {ComponentId}", id);
            return ex;
        }
    }

    public async Task<Result<IEnumerable<Component>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByIdsAsync started for {Count} component IDs", ids.Count());
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
                return Result<IEnumerable<Component>>.Ok(Enumerable.Empty<Component>());
            }

            var components = new List<Component>();

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

                var iterator = _container.GetItemQueryIterator<Component>(queryDef);
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    components.AddRange(response);
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetByIdsAsync completed. Found {Count} components", components.Count);
            }

            return components.Select(AppendSasToken).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving components by IDs");
            return ex;
        }
    }

    public async Task<Result<Component>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetByNameAsync started for Component name: {ComponentName}", name);
        }

        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.name = @name")
                .WithParameter("@name", name);

            var iterator = _container.GetItemQueryIterator<Component>(query);
            var components = new List<Component>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                components.AddRange(response);
            }

            if (components.Count == 0)
            {
                _logger.LogWarning("Component with name '{ComponentName}' not found", name);
                return new Exception($"Component with name '{name}' not found");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetByNameAsync completed successfully for Component name: {ComponentName}", name);
            }

            return AppendSasToken(components.First());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Component with name '{ComponentName}'", name);
            return ex;
        }
    }

    public async Task<Result<IEnumerable<Component>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetAllAsync started for components");
        }

        try
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<Component>(query);
            var components = new List<Component>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                components.AddRange(response);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetAllAsync completed. Found {Count} components", components.Count);
            }

            return components.Select(AppendSasToken).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all components");
            return ex;
        }
    }

    public async Task<Result> AddOrUpdateAsync(Component component, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("AddOrUpdateAsync started for Component ID: {ComponentId}", component.Id);
        }

        try
        {
            await _container.UpsertItemAsync(
                component,
                new PartitionKey(component.Id.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Component upserted successfully with ID: {ComponentId}",
                component.Id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("AddOrUpdateAsync completed for Component ID: {ComponentId}", component.Id);
            }

            return Result.Ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Component with ID {ComponentId} not found for upsert", component.Id);
            return new Exception($"Component with ID {component.Id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting Component with ID {ComponentId}", component.Id);
            return ex;
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("DeleteAsync started for Component ID: {ComponentId}", id);
        }

        try
        {
            // First, get the component to retrieve the partition key (boardId)
            var componentResult = await GetByIdAsync(id, cancellationToken);
            if (!componentResult.Success)
            {
                return componentResult.GetError();
            }

            var component = componentResult.GetOk();

            await _container.DeleteItemAsync<Component>(
                id.ToString(),
                new PartitionKey(component.Id.ToString()),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Component deleted successfully with ID: {ComponentId}", id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("DeleteAsync completed for Component ID: {ComponentId}", id);
            }

            return Result.Ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Component with ID {ComponentId} not found for deletion", id);
            return new Exception($"Component with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Component with ID {ComponentId}", id);
            return ex;
        }
    }

    private Component AppendSasToken(Component component)
    {
        if (string.IsNullOrWhiteSpace(_sasToken) || string.IsNullOrWhiteSpace(component.ImageUrl))
        {
            return component;
        }

        var token = _sasToken.TrimStart('?');
        var separator = component.ImageUrl.Contains("?", StringComparison.Ordinal) ? "&" : "?";
        return component with { ImageUrl = $"{component.ImageUrl}{separator}{token}" };
    }
}
