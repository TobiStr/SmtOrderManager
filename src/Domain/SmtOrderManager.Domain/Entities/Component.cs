using Newtonsoft.Json;
using SmtOrderManager.Domain.Primitives;

namespace SmtOrderManager.Domain.Entities;

/// <summary>
/// Represents a component that can be placed on a board.
/// </summary>
public record Component : Entity
{
    /// <summary>
    /// Gets the name of the component (must be unique).
    /// </summary>
    [JsonProperty("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the component.
    /// </summary>
    [JsonProperty("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets the quantity of this component.
    /// </summary>
    [JsonProperty("quantity")]
    public required int Quantity { get; init; }

    /// <summary>
    /// Gets the optional URL reference to the component image in blob storage.
    /// </summary>
    [JsonProperty("imageUrl")]
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Creates a new component with validation.
    /// </summary>
    public static Component Create(string name, string description, int quantity, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Component name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Component description cannot be empty.", nameof(description));

        if (quantity <= 0)
            throw new ArgumentException("Component quantity must be greater than zero.", nameof(quantity));

        return new Component
        {
            Id = UuidV7Generator.Generate(),
            Name = name,
            Description = description,
            Quantity = quantity,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }
}
