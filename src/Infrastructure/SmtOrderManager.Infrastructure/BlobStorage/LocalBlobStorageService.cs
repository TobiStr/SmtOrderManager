using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmtOrderManager.Domain.Services;

namespace SmtOrderManager.Infrastructure.BlobStorage;

/// <summary>
/// Local filesystem implementation of the blob storage service.
/// </summary>
public class LocalBlobStorageService : IBlobStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<LocalBlobStorageService> _logger;

    public LocalBlobStorageService(
        IOptions<LocalBlobStorageOptions> options,
        ILogger<LocalBlobStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(opts.RootPath))
        {
            throw new InvalidOperationException("Local blob storage root path is not configured");
        }

        _rootPath = Path.GetFullPath(opts.RootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<Result<Stream>> GetBlobAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Local GetBlobAsync started for blob: {BlobName}", blobName);
        }

        try
        {
            var path = GetPath(blobName);
            if (!File.Exists(path))
            {
                _logger.LogWarning("Local blob '{BlobName}' not found", blobName);
                return new FileNotFoundException($"Blob '{blobName}' not found");
            }

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Local GetBlobAsync completed for blob: {BlobName}", blobName);
            }

            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving local blob '{BlobName}'", blobName);
            return ex;
        }
    }

    public string GetBlobUrl(string blobName)
    {
        var path = GetPath(blobName);
        return path;
    }

    public async Task<Result> UploadAsync(string blobName, Stream content, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Local UploadAsync started for blob: {BlobName}", blobName);
        }

        try
        {
            var path = GetPath(blobName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            content.Position = 0;
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Local UploadAsync completed for blob: {BlobName}", blobName);
            }

            return Result.Ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading local blob '{BlobName}'", blobName);
            return ex;
        }
    }

    private string GetPath(string blobName)
    {
        var safeName = blobName.Replace("..", string.Empty).Replace("\\", "/");
        var combined = Path.Combine(_rootPath, safeName);
        return combined;
    }
}
