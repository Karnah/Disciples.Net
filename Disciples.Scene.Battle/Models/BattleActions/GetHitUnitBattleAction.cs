using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Нанесение урона юниту.
/// </summary>
internal class GetHitUnitBattleAction : UnitBattleAction
{
    /// <inheritdoc />
    public GetHitUnitBattleAction(BattleUnit targetUnit, int power, UnitAttackType attackType) : base(targetUnit, Enums.UnitActionType.GetHit)
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