using SmtOrderManager.Domain.Primitives;

namespace SmtOrderManager.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public record User : Entity
{
    /// <summary>
    /// Gets the email address of the user (must be unique).
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the hashed password of the user.
    /// </summary>
    public required string PasswordHash { get; init; }

    /// <summary>
    /// Gets the timestamp of the last login.
    /// </summary>
    public DateTime? LastLoginAt { get; init; }

    /// <summary>
    /// Gets the collection of orders created by this user.
    /// </summary>
    public IReadOnlyList<Order> Orders { get; init; } = Array.Empty<Order>();

    /// <summary>
    /// Creates a new user with validation.
    /// </summary>
    public static User Create(string email, string name, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Email format is invalid.", nameof(email));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        return new User
        {
            Id = UuidV7Generator.Generate(),
            Email = email.ToLowerInvariant(),
            Name = name,
            PasswordHash = passwordHash,
            LastLoginAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Orders = Array.Empty<Order>()
        };
    }

    /// <summary>
    /// Validates email format using MailAddress.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
