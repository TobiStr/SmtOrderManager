namespace SmtOrderManager.Domain.Primitives;

/// <summary>
/// Base record for all domain entities with audit properties.
/// </summary>
public abstract record Entity
{
    /// <summary>
    /// Gets the unique identifier for the entity (UUIDv7).
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the timestamp when the entity was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
