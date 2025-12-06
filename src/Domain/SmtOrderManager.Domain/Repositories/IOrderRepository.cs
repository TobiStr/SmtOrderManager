using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Domain.Repositories;

/// <summary>
/// Repository interface for Order entity operations.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets an order by its unique identifier with its associated boards and components.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the order with boards and components if found.</returns>
    Task<Result<Order>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple orders by their unique identifiers with their associated boards and components.
    /// </summary>
    /// <param name="ids">The collection of order IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the collection of orders with boards and components.</returns>
    Task<Result<IEnumerable<Order>>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates an order.
    /// </summary>
    /// <param name="order">The order to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> AddOrUpdateAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an order.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
