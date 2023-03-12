using System.ComponentModel.DataAnnotations.Schema;

namespace Disciples.Resources.Database.Dbf.Models;

/// <summary>
/// Текстовая информация для интерфейса.
/// </summary>
[Table("TApp")]
public class InterfaceTextResource : IEntity
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