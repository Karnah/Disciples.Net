using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Наложение эффекта на юнита.
/// </summary>
internal class EffectUnitBattleAction : UnitBattleAction
{
    /// <inheritdoc />
    public EffectUnitBattleAction(BattleUnit targetUnit, UnitAttackType attackType, UnitAttackSource attackSource, EffectDuration duration, Unit durationControlUnit, int? power, AnimationBattleAction? animationBattleAction)
        : base(targetUnit, UnitActionType.UnderEffect, attackType, power, animationBattleAction, attackSource)
    {
        Duration = duration;
        DurationControlUnit = durationControlUnit;
    }

    /// <summary>
    /// Продолжительность эффекта.
    /// </summary>
    public EffectDuration Duration { get; }

    /// <summary>
    /// Юнит, к ходу которого привязана длительность.
    /// </summary>
    public Unit DurationControlUnit { get; }

    /// <inheritdoc />
    /// <remarks>
    /// Когда эффект наложен, сила эффекта на портрете не отображается.
    /// </remarks>
    public override BattleUnitPortraitEventData GetUnitPortraitEventData()
    {
        return new BattleUnitPortraitEventData(ActionType, AttackType);
    }
}