using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Наложение эффекта на юнита.
/// </summary>
internal class EffectUnitBattleAction : UnitBattleAction
{
    /// <inheritdoc />
    public EffectUnitBattleAction(BattleUnit targetUnit, UnitAttackType attackType) : base(targetUnit, Enums.UnitActionType.UnderEffect)
    {
        AttackType = attackType;
    }

    /// <inheritdoc />
    public EffectUnitBattleAction(BattleUnit targetUnit, UnitAttackType attackType, int roundDuration, int? power)
        : base(targetUnit, Enums.UnitActionType.UnderEffect)
    {
        AttackType = attackType;
        RoundDuration = roundDuration;
        Power = power;
    }


    /// <summary>
    /// Атака из-за которой был наложен эффект.
    /// </summary>
    public UnitAttackType AttackType { get; }

    /// <summary>
    /// Продолжительность эффекта в раундах.
    /// </summary>
    public int RoundDuration { get; }

    /// <summary>
    /// Сила эффекта.
    /// </summary>
    public int? Power { get; }
}