using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Срабатывание эффекта при ходе юнита.
/// </summary>
internal class UnitTriggeredEffectAction : UnitBattleAction
{
    /// <summary>
    /// Создать объект типа <see cref="UnitTriggeredEffectAction" />.
    /// </summary>
    public UnitTriggeredEffectAction(BattleUnit targetUnit, UnitAttackType attackType, int? power, EffectDuration duration, AnimationBattleAction? animationBattleAction = null)
        : base(targetUnit, UnitActionType.TriggeredEffect, attackType, power, animationBattleAction)
    {
        Duration = duration;
    }

    /// <summary>
    /// Длительность эффекта после срабатывания.
    /// </summary>
    public EffectDuration Duration { get; }

    /// <inheritdoc />
    public override BattleUnitPortraitEventData GetUnitPortraitEventData()
    {
        return new BattleUnitPortraitEventData(ActionType, AttackType, Power, Duration);
    }
}