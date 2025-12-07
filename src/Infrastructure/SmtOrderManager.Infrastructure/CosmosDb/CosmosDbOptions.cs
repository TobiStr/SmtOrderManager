namespace SmtOrderManager.Infrastructure.CosmosDb;

/// <summary>
/// Configuration options for Cosmos DB.
/// </summary>
public class CosmosDbOptions
{
    /// <summary>
    /// Gets or sets the Cosmos DB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = "SmtOrderManager";

    /// <summary>
    /// Gets or sets the container configuration.
    /// </summary>
    public CosmosDbContainers Containers { get; set; } = new();
}

/// <summary>
/// Container names configuration for Cosmos DB.
/// </summary>
public class CosmosDbContainers
{
    /// <summary>
    /// Gets or sets the Orders container name.
    /// </summary>
    public string Orders { get; set; } = "Orders";

    /// <summary>
    /// Gets or sets the Boards container name.
    /// </summary>
    public string Boards { get; set; } = "Boards";

    /// <summary>
    /// Gets or sets the Components container name.
    /// </summary>
    public string Components { get; set; } = "Components";

    /// <summary>
    /// Gets or sets the Users container name.
    /// </summary>
    public string Users { get; set; } = "Users";
}
