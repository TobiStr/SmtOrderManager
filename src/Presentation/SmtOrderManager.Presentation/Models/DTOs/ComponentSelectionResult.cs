namespace SmtOrderManager.Presentation.Models.DTOs;

/// <summary>
/// Ergebnis einer Component-Auswahl im ComponentPicker
/// </summary>
public class ComponentSelectionResult
{
    public Guid ComponentId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string? Reference { get; set; }
    public string? Comment { get; set; }
}
