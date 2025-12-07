using Newtonsoft.Json;

namespace SmtOrderManager.Domain.Primitives;

/// <summary>
/// Represents a reference to another entity with an associated quantity.
/// </summary>
public record QuantizedId
{
    [JsonProperty("id")]
    public Guid Id { get; init; }

    [JsonProperty("quantity")]
    public long Quantity { get; init; }

    public QuantizedId(Guid id, long quantity)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Id = id;
        Quantity = quantity;
    }
}
