using SmtOrderManager.Domain.Primitives;

namespace SmtOrderManager.Domain.Entities;

/// <summary>
/// Represents a printed circuit board (PCB) that contains components.
/// </summary>
public record Board : Entity
{
    /// <summary>
    /// Gets the name of the board (must be unique).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the board.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the length of the board in millimeters.
    /// </summary>
    public required decimal Length { get; init; }

    /// <summary>
    /// Gets the width of the board in millimeters.
    /// </summary>
    public required decimal Width { get; init; }

    /// <summary>
    /// Gets the ID of the order this board belongs to.
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// Gets the collection of components on this board.
    /// </summary>
    public IReadOnlyList<Component> Components { get; init; } = Array.Empty<Component>();

    /// <summary>
    /// Creates a new board with validation.
    /// </summary>
    public static Board Create(string name, string description, decimal length, decimal width, Guid orderId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Board name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Board description cannot be empty.", nameof(description));

        if (length <= 0)
            throw new ArgumentException("Board length must be greater than zero.", nameof(length));

        if (width <= 0)
            throw new ArgumentException("Board width must be greater than zero.", nameof(width));

        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID cannot be empty.", nameof(orderId));

        return new Board
        {
            Id = UuidV7Generator.Generate(),
            Name = name,
            Description = description,
            Length = length,
            Width = width,
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Components = Array.Empty<Component>()
        };
    }
}
