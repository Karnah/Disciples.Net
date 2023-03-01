using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Атака юнита.
/// </summary>
public class AttackUnitBattleAction : UnitBattleAction
{
    /// <inheritdoc />
    public AttackUnitBattleAction(BattleUnit targetUnit, int power, AttackClass attackClass) : base(targetUnit, Enums.UnitActionType.GetHit)
    {
        Power = power;
        AttackClass = attackClass;
    }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int Power { get; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public AttackClass AttackClass { get; }
}