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

    /// <summary>
    /// Uploads a blob to storage.
    /// </summary>
    /// <param name="blobName">Target blob name.</param>
    /// <param name="content">Stream content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> UploadAsync(string blobName, Stream content, CancellationToken cancellationToken = default);
}
