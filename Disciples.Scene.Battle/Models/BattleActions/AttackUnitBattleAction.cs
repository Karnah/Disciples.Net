using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Атака юнита.
/// </summary>
internal class AttackUnitBattleAction : UnitBattleAction
{
    /// <inheritdoc />
    public AttackUnitBattleAction(BattleUnit targetUnit, int power, UnitAttackType attackType) : base(targetUnit, Enums.UnitActionType.GetHit)
    {
        Power = power;
        AttackType = attackType;
    }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int Power { get; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType AttackType { get; }
}