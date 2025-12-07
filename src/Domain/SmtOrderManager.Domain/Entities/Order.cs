using Newtonsoft.Json;
using SmtOrderManager.Domain.Primitives;
using SmtOrderManager.Domain.Enums;

namespace SmtOrderManager.Domain.Entities;

/// <summary>
/// Represents an order in the SMT manufacturing system (root aggregate).
/// </summary>
public record Order : Entity
{
    /// <summary>
    /// Gets the description of the order.
    /// </summary>
    [JsonProperty("description")]
    public required string Description { get; init; }

    /// <summary>
    /// Gets the date when the order was placed.
    /// </summary>
    [JsonProperty("orderDate")]
    public required DateTime OrderDate { get; init; }

    /// <summary>
    /// Gets the current status of the order.
    /// </summary>
    [JsonProperty("status")]
    public required OrderStatus Status { get; init; }

    /// <summary>
    /// Gets the ID of the user who created this order.
    /// </summary>
    [JsonProperty("userId")]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the collection of board IDs with quantities in this order (persisted to database).
    /// </summary>
    [JsonProperty("boardIds")]
    public IReadOnlyList<QuantizedId> BoardIds { get; init; } = Array.Empty<QuantizedId>();

    /// <summary>
    /// Gets the collection of boards in this order (populated on retrieval, not persisted).
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Board> Boards { get; init; } = Array.Empty<Board>();

    /// <summary>
    /// Creates a new order with validation.
    /// </summary>
    public static Order Create(string description, DateTime orderDate, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Order description cannot be empty.", nameof(description));

        if (orderDate == default)
            throw new ArgumentException("Order date cannot be default.", nameof(orderDate));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new Order
        {
            Id = UuidV7Generator.Generate(),
            Description = description,
            OrderDate = orderDate,
            Status = OrderStatus.Draft,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            BoardIds = Array.Empty<QuantizedId>(),
            Boards = Array.Empty<Board>()
        };
    }

    /// <summary>
    /// Adds a board to the order by creating a new instance.
    /// </summary>
    public Order AddBoard(Guid boardId, long quantity, Board? board = null)
    {
        if (boardId == Guid.Empty)
            throw new ArgumentException("Board ID cannot be empty.", nameof(boardId));

        if (BoardIds.Any(b => b.Id == boardId))
            throw new InvalidOperationException("Board already exists in this order.");

        var newBoardIds = new List<QuantizedId>(BoardIds) { new QuantizedId(boardId, quantity) };
        var newBoards = board is null
            ? new List<Board>(Boards)
            : new List<Board>(Boards) { board };

        return this with
        {
            BoardIds = newBoardIds,
            Boards = newBoards,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Order UpdateBoardQuantity(Guid boardId, long quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (!BoardIds.Any(b => b.Id == boardId))
        {
            throw new InvalidOperationException("Board not found in this order.");
        }

        var updated = BoardIds
            .Select(b => b.Id == boardId ? new QuantizedId(boardId, quantity) : b)
            .ToList();

        return this with
        {
            BoardIds = updated,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Removes a board from the order by creating a new instance.
    /// </summary>
    public Order RemoveBoard(Guid boardId)
    {
        if (!BoardIds.Any(b => b.Id == boardId))
            throw new InvalidOperationException("Board not found in this order.");

        var newBoardIds = BoardIds.Where(b => b.Id != boardId).ToList();
        var newBoards = Boards.Where(b => b.Id != boardId).ToList();

        return this with
        {
            BoardIds = newBoardIds,
            Boards = newBoards,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
