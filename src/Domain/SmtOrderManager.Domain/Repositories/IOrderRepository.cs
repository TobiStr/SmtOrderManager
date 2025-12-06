using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Domain.Repositories;

/// <summary>
/// Repository interface for Order entity operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets an order by its unique identifier.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the order if found.</returns>
    Task<Result<Order>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all orders for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the collection of orders.</returns>
    Task<Result<IEnumerable<Order>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all orders with their associated boards.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the collection of orders with boards.</returns>
    Task<Result<IEnumerable<Order>>> GetWithBoardsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches orders by description.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the collection of matching orders.</returns>
    Task<Result<IEnumerable<Order>>> SearchAsync(string searchTerm, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new order.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> AddAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="order">The order to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> UpdateAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an order.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
