namespace SmtOrderManager.Presentation.Models.DTOs;

/// <summary>
/// DTO für Component-Daten im UI
/// </summary>
public class ComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Zusätzliche UI-Properties
    public bool IsSelected { get; set; }
    public string DisplayName => $"{Name} ({Description})";
}
