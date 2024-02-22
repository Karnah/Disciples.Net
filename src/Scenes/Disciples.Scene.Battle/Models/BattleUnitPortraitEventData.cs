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
    public BattleUnitPortraitEventData(UnitActionType unitActionType)
    {
        UnitActionType = unitActionType;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleUnitPortraitEventData" />.
    /// </summary>
    public BattleUnitPortraitEventData(CalculatedAttackResult attackResult)
    {
        UnitActionType = UnitActionType.Attacked;
        AttackType = attackResult.AttackType;
        Power = attackResult.Power;
        CriticalDamage = attackResult.CriticalDamage;
        EffectDuration = attackResult.EffectDuration;
        IsEffectTriggered = false;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleUnitPortraitEventData" />.
    /// </summary>
    public BattleUnitPortraitEventData(CalculatedEffectResult effectResult)
    {
        UnitActionType = UnitActionType.Attacked;
        AttackType = effectResult.Effect.AttackType;
        Power = effectResult.Power;
        CriticalDamage = null;
        EffectDuration = effectResult.NewDuration;
        IsEffectTriggered = true;
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