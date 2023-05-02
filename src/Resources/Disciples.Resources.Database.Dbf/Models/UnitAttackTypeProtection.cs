using System.ComponentModel.DataAnnotations.Schema;
using Disciples.Resources.Database.Dbf.Enums;

namespace Disciples.Resources.Database.Dbf.Models;

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
    /// Тип атаки.
    /// </summary>
    [Column("IMMUNITY")]
    public UnitAttackType UnitAttackType { get; init; }

    /// <summary>
    /// Категория защиты.
    /// </summary>
    [Column("IMMUNECAT")]
    public ProtectionCategory ProtectionCategory { get; init; }
}