using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Данные для события, которое должно отображаться на портрете юнита.
/// </summary>
internal readonly ref struct BattleUnitPortraitEventData
{
    /// <summary>
    /// Создать объект типа <see cref="BattleUnitPortraitEventData" />.
    /// </summary>
    public BattleUnitPortraitEventData(UnitActionType unitActionType, UnitAttackType? attackType, int? power,
        int? criticalDamage, EffectDuration? effectDuration, bool isEffectTriggered)
    {
        UnitActionType = unitActionType;
        AttackType = attackType;
        Power = power;
        EffectDuration = effectDuration;
        IsEffectTriggered = isEffectTriggered;
        CriticalDamage = criticalDamage;
    }

    /// <summary>
    /// Действие юнита.
    /// </summary>
    public UnitActionType UnitActionType { get; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType? AttackType { get; }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int? Power { get; }

    /// <summary>
    /// Критический урон.
    /// </summary>
    public int? CriticalDamage { get; }

    /// <summary>
    /// Длительность эффекта.
    /// </summary>
    public EffectDuration? EffectDuration { get; }

    /// <summary>
    /// Признак, что это срабатывание эффекта на ходу юнита.
    /// </summary>
    public bool IsEffectTriggered { get; }
}