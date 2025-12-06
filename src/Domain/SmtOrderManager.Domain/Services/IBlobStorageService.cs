namespace SmtOrderManager.Domain.Services;

/// <summary>
/// Service interface for blob storage operations.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Gets a blob (image) as a stream from the storage container.
    /// </summary>
    /// <param name="blobName">The name of the blob to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the blob stream if found.</returns>
    Task<Result<Stream>> GetBlobAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the URL for a blob.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <returns>The URL to access the blob.</returns>
    string GetBlobUrl(string blobName);
}
