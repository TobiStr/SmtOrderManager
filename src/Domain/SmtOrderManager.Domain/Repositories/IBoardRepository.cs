using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Domain.Repositories;

/// <summary>
/// Repository interface for Board entity operations.
/// </summary>
public interface IBoardRepository
{
    /// <summary>
    /// Gets a board by its unique identifier with its associated components.
    /// </summary>
    /// <param name="id">The board ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the board with components if found.</returns>
    Task<Result<Board>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple boards by their unique identifiers with their associated components.
    /// </summary>
    /// <param name="ids">The collection of board IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the collection of boards with components.</returns>
    Task<Result<IEnumerable<Board>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a board by its unique name with its associated components.
    /// </summary>
    /// <param name="name">The board name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the board with components if found.</returns>
    Task<Result<Board>> GetByNameAsync(string name, CancellationToken cancellationToken = default);

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
