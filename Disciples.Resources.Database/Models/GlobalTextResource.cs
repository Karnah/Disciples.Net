using System.ComponentModel.DataAnnotations.Schema;

namespace Disciples.Resources.Database.Models;

/// <summary>
/// Общая текстовая информация.
/// </summary>
[Table("Tglobal")]
public class GlobalTextResource : IEntity
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    [Column("TXT_ID")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Текст.
    /// </summary>
    [Column("TEXT")]
    public string Text { get; init; } = null!;

    /// <summary>
    /// Верифицирована ли запись.
    /// </summary>
    /// <remarks>
    /// Никак не используется.
    /// </remarks>
    [Column("VERIFIED")]
    public bool IsVerified { get; init; }

    /// <summary>
    /// Контекст записи.
    /// </summary>
    /// <remarks>
    /// Никак не используется.
    /// </remarks>
    [Column("CONTEXT")]
    public string? Context { get; init; }
}