using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmtOrderManager.Domain.Services;

namespace SmtOrderManager.Infrastructure.BlobStorage;

/// <summary>
/// Azure Blob Storage implementation of the blob storage service.
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        IOptions<BlobStorageOptions> options,
        ILogger<BlobStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(opts.ConnectionString))
        {
            throw new InvalidOperationException("Blob Storage connection string is not configured");
        }

        var blobServiceClient = new BlobServiceClient(opts.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(opts.ContainerName);
    }

    public async Task<Result<Stream>> GetBlobAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetBlobAsync started for blob: {BlobName}", blobName);
        }

        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                _logger.LogWarning("Blob '{BlobName}' not found", blobName);
                return new Exception($"Blob '{blobName}' not found");
            }

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("GetBlobAsync completed successfully for blob: {BlobName}", blobName);
            }

            return response.Value.Content;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Blob '{BlobName}' not found", blobName);
            return new Exception($"Blob '{blobName}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blob '{BlobName}'", blobName);
            return ex;
        }
    }

    public string GetBlobUrl(string blobName)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetBlobUrl called for blob: {BlobName}", blobName);
        }

        var blobClient = _containerClient.GetBlobClient(blobName);
        var url = blobClient.Uri.ToString();

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("GetBlobUrl completed for blob: {BlobName}, URL: {Url}", blobName, url);
        }

        return url;
    }

    public async Task<Result> UploadAsync(string blobName, Stream content, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("UploadAsync started for blob: {BlobName}", blobName);
        }

        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            Stream uploadStream = content;

            if (content.CanSeek)
            {
                content.Position = 0;
            }
            else
            {
                var buffered = new MemoryStream();
                await content.CopyToAsync(buffered, cancellationToken);
                buffered.Position = 0;
                uploadStream = buffered;
            }

            await blobClient.UploadAsync(uploadStream, overwrite: true, cancellationToken);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("UploadAsync completed successfully for blob: {BlobName}", blobName);
            }

            return Result.Ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blob '{BlobName}'", blobName);
            return ex;
        }
    }
}
