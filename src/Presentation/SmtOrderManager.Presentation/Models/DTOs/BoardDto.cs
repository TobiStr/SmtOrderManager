namespace SmtOrderManager.Presentation.Models.DTOs;

/// <summary>
/// DTO für Board-Daten im UI
/// </summary>
public class BoardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public Guid? OrderId { get; set; }
    public List<BoardComponentDto> Components { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // UI Helpers
    public string Dimensions => $"{Length} × {Width} mm";
    public int ComponentCount => Components.Count;
    public string Summary => $"{Name} ({ComponentCount} Components)";
}
