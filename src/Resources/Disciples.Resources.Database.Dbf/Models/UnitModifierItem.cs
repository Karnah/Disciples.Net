using System.ComponentModel.DataAnnotations.Schema;
using Disciples.Resources.Database.Dbf.Enums;

namespace Disciples.Resources.Database.Dbf.Models;

/// <summary>
/// Модификатор одной характеристики или способности юнита.
/// </summary>
[Table("GmodifL")]
public class UnitModifierItem : IEntity
{
    /// <summary>
    /// Идентификатор модификатора.
    /// </summary>
    /// <remarks>
    /// Идентификатор НЕ уникален.
    /// </remarks>
    [Column("BELONGS_TO")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Идентификатор для описания модификатора.
    /// </summary>
    [Column("DESC")]
    public string DescriptionTextId { get; init; } = null!;

    /// <summary>
    /// Тип модификатора.
    /// </summary>
    [Column("TYPE")]
    public UnitModifierItemType ModifierItemType { get; init; }

    /// <summary>
    /// Процент, на который изменяется характеристика.
    /// </summary>
    [Column("PERCENT")]
    public int? Percent { get; init; }

    /// <summary>
    /// Численное значение на которое изменяется характеристика.
    /// </summary>
    /// <remarks>
    /// Заполнено если <see cref="UnitModifierItemType.ScoutPoints" />, <see cref="UnitModifierItemType.Leadership" />,
    /// <see cref="UnitModifierItemType.Armor" />, <see cref="UnitModifierItemType.HitPoints" />
    /// </remarks>
    [Column("NUMBER")]
    public int? Number { get; init; }

    /// <summary>
    /// Тип способности лидера.
    /// </summary>
    /// <remarks>
    /// Заполнено, если <see cref="UnitModifierItemType.LeaderAbility" />.
    /// </remarks>
    [Column("ABILITY")]
    public LeaderAbilityType? LeaderAbilityType { get; init; }

    /// <summary>
    /// Тип иммунитета/защиты.
    /// </summary>
    /// <remarks>
    /// Если <see cref="UnitModifierItemType.AttackSourceProtection" />, то <see cref="UnitAttackSource" />.
    /// Если <see cref="UnitModifierItemType.AttackTypeProtection" />, то <see cref="UnitAttackType" />.
    /// </remarks>
    [Column("IMMUNITY")]
    public int? ProtectionType { get; init; }

    /// <summary>
    /// Категория защиты от атаки.
    /// </summary>
    /// <remarks>
    /// Заполнено, если <see cref="UnitModifierItemType.AttackSourceProtection" /> или <see cref="UnitModifierItemType.AttackTypeProtection" />.
    /// </remarks>
    [Column("IMMUNECAT")]
    public ProtectionCategory? ProtectionCategory { get; init; }

    /// <summary>
    /// Тип ландшафта.
    /// </summary>
    /// <remarks>
    /// Заполнено, если <see cref="UnitModifierItemType.MoveGround" />.
    /// </remarks>
    [Column("MOVE")]
    public GroundType? GroundType { get; init; }
}