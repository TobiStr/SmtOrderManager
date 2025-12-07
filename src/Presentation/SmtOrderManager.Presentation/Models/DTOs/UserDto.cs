namespace SmtOrderManager.Presentation.Models.DTOs;

/// <summary>
/// DTO für User-Daten im UI
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // UI Helpers
    public string DisplayName => Name;
    public string LastLoginDisplay => LastLoginAt?.ToString("dd.MM.yyyy HH:mm") ?? "Noch nie";
}
