using SmtOrderManager.Domain.Enums;

namespace SmtOrderManager.Presentation.Models.DTOs;

/// <summary>
/// DTO für Order-Daten im UI
/// </summary>
public record OrderDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public List<BoardDto> Boards { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // UI Helpers
    public int BoardCount => Boards.Count;
    public bool CanSubmit => Status == OrderStatus.Draft;
    public bool CanCancel => Status is OrderStatus.Draft or OrderStatus.Submitted;
    public bool IsEditable => Status is OrderStatus.Draft;
    public string StatusDisplayText => Status switch
    {
        OrderStatus.Draft => "📝 Entwurf",
        OrderStatus.Submitted => "📤 Eingereicht",
        OrderStatus.InProduction => "🔧 In Produktion",
        OrderStatus.Completed => "✅ Fertig",
        OrderStatus.Cancelled => "❌ Storniert",
        OrderStatus.Archived => "📦 Archiviert",
        _ => Status.ToString()
    };
}
