using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Domain.Repositories;

/// <summary>
/// Repository interface for Board entity operations.
/// </summary>
public interface IBoardRepository
{
    /// <summary>
    /// Gets a board by its unique identifier.
    /// </summary>
    /// <param name="id">The board ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the board if found.</returns>
    Task<Result<Board>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a board by its unique name.
    /// </summary>
    /// <param name="name">The board name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the board if found.</returns>
    Task<Result<Board>> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all boards for a specific order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the collection of boards.</returns>
    Task<Result<IEnumerable<Board>>> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all boards with their associated components.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the collection of boards with components.</returns>
    Task<Result<IEnumerable<Board>>> GetWithComponentsAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new board.
    /// </summary>
    /// <param name="board">The board to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> AddAsync(Board board, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing board.
    /// </summary>
    /// <param name="board">The board to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> UpdateAsync(Board board, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a board.
    /// </summary>
    /// <param name="id">The ID of the board to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
