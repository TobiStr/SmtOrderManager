using Newtonsoft.Json;
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
    [JsonProperty("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the board.
    /// </summary>
    [JsonProperty("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets the length of the board in millimeters.
    /// </summary>
    [JsonProperty("length")]
    public required decimal Length { get; init; }

    /// <summary>
    /// Gets the width of the board in millimeters.
    /// </summary>
    [JsonProperty("width")]
    public required decimal Width { get; init; }

    /// <summary>
    /// Gets the collection of component IDs with quantities on this board (persisted to database).
    /// </summary>
    [JsonProperty("componentIds")]
    public IReadOnlyList<QuantizedId> ComponentIds { get; init; } = Array.Empty<QuantizedId>();

    /// <summary>
    /// Gets the collection of components on this board (populated on retrieval, not persisted).
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Component> Components { get; init; } = Array.Empty<Component>();

    /// <summary>
    /// Creates a new board with validation.
    /// </summary>
    public static Board Create(string name, string description, decimal length, decimal width)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Board name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Board description cannot be empty.", nameof(description));

        if (length <= 0)
            throw new ArgumentException("Board length must be greater than zero.", nameof(length));

        if (width <= 0)
            throw new ArgumentException("Board width must be greater than zero.", nameof(width));

        return new Board
        {
            Id = UuidV7Generator.Generate(),
            Name = name,
            Description = description,
            Length = length,
            Width = width,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            ComponentIds = Array.Empty<QuantizedId>(),
            Components = Array.Empty<Component>()
        };
    }

    public Board AddComponent(Guid componentId, long quantity, Component? component = null)
    {
        if (ComponentIds.Any(c => c.Id == componentId))
        {
            throw new InvalidOperationException("Component already exists on this board.");
        }

        var newComponentIds = new List<QuantizedId>(ComponentIds)
        {
            new QuantizedId(componentId, quantity)
        };

        var newComponents = component is null
            ? new List<Component>(Components)
            : new List<Component>(Components) { component };

        return this with
        {
            ComponentIds = newComponentIds,
            Components = newComponents,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Board UpdateComponentQuantity(Guid componentId, long quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (!ComponentIds.Any(c => c.Id == componentId))
        {
            throw new InvalidOperationException("Component not found on this board.");
        }

        var updated = ComponentIds
            .Select(c => c.Id == componentId ? new QuantizedId(componentId, quantity) : c)
            .ToList();

        return this with
        {
            ComponentIds = updated,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Board RemoveComponent(Guid componentId)
    {
        if (!ComponentIds.Any(c => c.Id == componentId))
        {
            throw new InvalidOperationException("Component not found on this board.");
        }

        var newComponentIds = ComponentIds.Where(c => c.Id != componentId).ToList();
        var newComponents = Components.Where(c => c.Id != componentId).ToList();

        return this with
        {
            ComponentIds = newComponentIds,
            Components = newComponents,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
