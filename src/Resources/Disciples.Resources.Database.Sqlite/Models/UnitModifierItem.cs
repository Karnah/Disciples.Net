using Disciples.Resources.Database.Sqlite.Enums;

namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Модификатор характеристики или способности юнита.
/// </summary>
public class UnitModifierItem : IEntity
{
    /// <summary>
    /// Идентификатор модификатора.
    /// </summary>
    /// <remarks>
    /// Идентификатор НЕ уникален.
    /// </remarks>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Модификатор.
    /// </summary>
    public UnitModifier UnitModifier { get; init; } = null!;

    /// <summary>
    /// Описание модификатора.
    /// </summary>
    public GlobalTextResource Description { get; init; } = null!;

    /// <summary>
    /// Тип модификатора.
    /// </summary>
    public UnitModifierItemType ModifierItemType { get; init; }

    /// <summary>
    /// Процент, на который изменяется характеристика.
    /// </summary>
    public int? Percent { get; init; }

    /// <summary>
    /// Численное значение на которое изменяется характеристика.
    /// </summary>
    /// <remarks>
    /// Заполнено если <see cref="UnitModifierItemType.ScoutPoints" />, <see cref="UnitModifierItemType.Leadership" />,
    /// <see cref="UnitModifierItemType.Armor" />, <see cref="UnitModifierItemType.HitPoints" />
    /// </remarks>
    public int? Number { get; init; }

    /// <summary>
    /// Тип способности лидера.
    /// </summary>
    /// <remarks>
    /// Заполнено, если <see cref="UnitModifierItemType.LeaderAbility" />.
    /// </remarks>
    public LeaderAbilityType? LeaderAbilityType { get; init; }

    /// <summary>
    /// Тип иммунитета/защиты.
    /// </summary>
    /// <remarks>
    /// Если <see cref="UnitModifierItemType.AttackSourceProtection" />, то <see cref="UnitAttackSource" />.
    /// Если <see cref="UnitModifierItemType.AttackTypeProtection" />, то <see cref="UnitAttackType" />.
    /// </remarks>
    public int? ProtectionType { get; init; }

    /// <summary>
    /// Категория защиты от атаки.
    /// </summary>
    /// <remarks>
    /// Заполнено, если <see cref="UnitModifierItemType.AttackSourceProtection" /> или <see cref="UnitModifierItemType.AttackTypeProtection" />.
    /// </remarks>
    public ProtectionCategory? ProtectionCategory { get; init; }

    /// <summary>
    /// Тип ландшафта.
    /// </summary>
    /// <remarks>
    /// Заполнено, если <see cref="UnitModifierItemType.MoveGround" />.
    /// </remarks>
    public GroundType? GroundType { get; init; }
}