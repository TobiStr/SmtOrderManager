using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Presentation.Models.DTOs;

namespace SmtOrderManager.Presentation.Models.Mappers;

/// <summary>
/// Mapper zwischen Domain-Entities und UI-DTOs
/// NUR für User (wegen PasswordHash-Sicherheit)
/// Alle anderen Entities werden direkt verwendet!
/// </summary>
public static class EntityMapper
{
    /// <summary>
    /// User → UserDto (OHNE PasswordHash!)
    /// </summary>
    public static UserDto ToDto(this User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            Email = entity.Email,
            Name = entity.Name,
            LastLoginAt = entity.LastLoginAt,
            CreatedAt = entity.CreatedAt
        };
    }

    // Hinweis: Order, Board, Component werden direkt als Domain-Entities verwendet!
    // Nur User hat DTO wegen PasswordHash-Sicherheit.
}
