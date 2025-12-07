namespace SmtOrderManager.Infrastructure.BlobStorage;

/// <summary>
/// Options for local blob storage implementation.
/// </summary>
public class LocalBlobStorageOptions
{
    /// <summary>
    /// Gets or sets the root path for storing blobs locally.
    /// </summary>
    public string RootPath { get; set; } = "./wwwroot/uploads";
}
