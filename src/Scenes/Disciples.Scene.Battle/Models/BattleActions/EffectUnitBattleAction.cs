using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Наложение эффекта на юнита.
/// </summary>
internal class EffectUnitBattleAction : UnitBattleAction
{
    /// <inheritdoc />
    public EffectUnitBattleAction(BattleUnit targetUnit, UnitAttackType attackType, int roundDuration, int? power, AnimationBattleAction? animationBattleAction)
        : base(targetUnit, UnitActionType.UnderEffect, attackType, power, animationBattleAction)
    {
        RoundDuration = roundDuration;
    }

    /// <summary>
    /// Продолжительность эффекта в раундах.
    /// </summary>
    public int RoundDuration { get; }

    /// <inheritdoc />
    /// <remarks>
    /// Когда эффект наложен, сила эффекта на портрете не отображается.
    /// </remarks>
    public override BattleUnitPortraitEventData GetUnitPortraitEventData()
    {
        return new BattleUnitPortraitEventData(ActionType, AttackType, null);
    }
}