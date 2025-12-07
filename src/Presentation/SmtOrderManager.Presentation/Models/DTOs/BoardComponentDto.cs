namespace SmtOrderManager.Presentation.Models.DTOs;

/// <summary>
/// DTO für die Zuordnung einer Component zu einem Board
/// </summary>
public class BoardComponentDto
{
    public Guid ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? ImageUrl { get; set; }
    public string? Comment { get; set; }

    // UI Helper
    public string DisplayText => $"{ComponentName} (Anzahl: {Quantity}{(string.IsNullOrWhiteSpace(Reference) ? "" : $", Ref: {Reference}")})";
}
