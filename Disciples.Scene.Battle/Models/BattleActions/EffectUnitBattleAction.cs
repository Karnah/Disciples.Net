using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Наложение эффекта на юнита.
/// </summary>
public class EffectUnitBattleAction : UnitBattleAction
{
    /// <inheritdoc />
    public EffectUnitBattleAction(BattleUnit targetUnit, AttackClass attackClass) : base(targetUnit, Enums.UnitActionType.UnderEffect)
    {
        AttackClass = attackClass;
    }

    /// <inheritdoc />
    public EffectUnitBattleAction(BattleUnit targetUnit, AttackClass attackClass, int roundDuration, int? power)
        : base(targetUnit, Enums.UnitActionType.UnderEffect)
    {
        AttackClass = attackClass;
        RoundDuration = roundDuration;
        Power = power;
    }


    /// <summary>
    /// Атака из-за которой был наложен эффект.
    /// </summary>
    public AttackClass AttackClass { get; }

    /// <summary>
    /// Продолжительность эффекта в раундах.
    /// </summary>
    public int RoundDuration { get; }

    /// <summary>
    /// Сила эффекта.
    /// </summary>
    public int? Power { get; }
}