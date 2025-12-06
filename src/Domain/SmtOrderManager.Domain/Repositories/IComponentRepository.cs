using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Domain.Repositories;

/// <summary>
/// Repository interface for Component entity operations.
/// </summary>
public interface IComponentRepository
{
    /// <summary>
    /// Gets a component by its unique identifier.
    /// </summary>
    /// <param name="id">The component ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the component if found.</returns>
    Task<Result<Component>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a component by its unique name.
    /// </summary>
    /// <param name="name">The component name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the component if found.</returns>
    Task<Result<Component>> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all components for a specific board.
    /// </summary>
    /// <param name="boardId">The board ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the collection of components.</returns>
    Task<Result<IEnumerable<Component>>> GetByBoardIdAsync(Guid boardId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new component.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> AddAsync(Component component, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing component.
    /// </summary>
    /// <param name="component">The component to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> UpdateAsync(Component component, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a component.
    /// </summary>
    /// <param name="id">The ID of the component to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
