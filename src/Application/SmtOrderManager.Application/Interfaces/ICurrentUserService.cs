namespace SmtOrderManager.Application.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the ID of the current authenticated user.
    /// </summary>
    /// <returns>The user ID, or null if not authenticated.</returns>
    Guid? GetUserId();

    /// <summary>
    /// Gets the email of the current authenticated user.
    /// </summary>
    /// <returns>The email address, or null if not authenticated.</returns>
    string? GetEmail();

    /// <summary>
    /// Determines whether the current user is authenticated.
    /// </summary>
    /// <returns>True if authenticated; otherwise, false.</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Determines whether the current user is in the specified role.
    /// </summary>
    /// <param name="role">The role name to check.</param>
    /// <returns>True if the user is in the role; otherwise, false.</returns>
    bool IsInRole(string role);
}
