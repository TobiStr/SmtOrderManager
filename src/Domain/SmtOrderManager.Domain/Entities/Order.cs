using SmtOrderManager.Domain.Primitives;

namespace SmtOrderManager.Domain.Entities;

/// <summary>
/// Represents an order in the SMT manufacturing system (root aggregate).
/// </summary>
public record Order : Entity
{
    /// <summary>
    /// Gets the description of the order.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the date when the order was placed.
    /// </summary>
    public required DateTime OrderDate { get; init; }

    /// <summary>
    /// Gets the ID of the user who created this order.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the collection of boards in this order.
    /// </summary>
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
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Boards = Array.Empty<Board>()
        };
    }

    /// <summary>
    /// Adds a board to the order by creating a new instance.
    /// </summary>
    public Order AddBoard(Board board)
    {
        if (board == null)
            throw new ArgumentNullException(nameof(board));

        if (board.OrderId != Id)
            throw new InvalidOperationException("Board does not belong to this order.");

        if (Boards.Any(b => b.Id == board.Id))
            throw new InvalidOperationException("Board already exists in this order.");

        var newBoards = new List<Board>(Boards) { board };

        return this with
        {
            Boards = newBoards,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Removes a board from the order by creating a new instance.
    /// </summary>
    public Order RemoveBoard(Guid boardId)
    {
        var board = Boards.FirstOrDefault(b => b.Id == boardId);
        if (board == null)
            throw new InvalidOperationException("Board not found in this order.");

        var newBoards = Boards.Where(b => b.Id != boardId).ToList();

        return this with
        {
            Boards = newBoards,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
