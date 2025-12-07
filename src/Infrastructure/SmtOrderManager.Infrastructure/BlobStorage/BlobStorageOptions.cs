namespace SmtOrderManager.Infrastructure.BlobStorage;

/// <summary>
/// Configuration options for Azure Blob Storage.
/// </summary>
public class BlobStorageOptions
{
    /// <summary>
    /// Gets or sets the connection string for Azure Blob Storage.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the container name for component images.
    /// </summary>
    public string ContainerName { get; set; } = "component-images";
}
