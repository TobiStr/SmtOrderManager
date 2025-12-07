namespace SmtOrderManager.Infrastructure.BlobStorage;

/// <summary>
/// Options controlling how image URLs are constructed for components.
/// </summary>
public class ImageUrlOptions
{
    /// <summary>
    /// Optional SAS token appended to blob URLs.
    /// </summary>
    public string? SasToken { get; set; }
}
