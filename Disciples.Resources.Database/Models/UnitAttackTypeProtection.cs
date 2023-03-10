using Disciples.Resources.Database.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Disciples.Resources.Database.Models;

/// <summary>
/// Защита юнита от типа атаки.
/// </summary>
[Table("GimmuC")]
public class UnitAttackTypeProtection : IEntity
{
    /// <summary>
    /// Идентификатор юнита, который имеет защиту.
    /// </summary>
    /// <remarks>
    /// Идентификатор НЕ уникален.
    /// </remarks>
    [Column("UNIT_ID")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Источник атаки.
    /// </summary>
    [Column("IMMUNITY")]
    public UnitAttackType UnitAttackType { get; init; }

    /// <summary>
    /// Категория защиты.
    /// </summary>
    [Column("IMMUNECAT")]
    public ProtectionCategory ProtectionCategory { get; init; }
}