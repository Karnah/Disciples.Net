using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие, которое совершается юнитом.
/// </summary>
public class UnitBattleAction : BaseTimerBattleAction
{
    /// <summary>
    /// Продолжительность воздействия
    /// </summary>
    private const int TOUCH_UNIT_ACTION_DURATION = 1000;

    /// <inheritdoc />
    public UnitBattleAction(BattleUnit targetUnit, UnitActionType unitActionType) : base(TOUCH_UNIT_ACTION_DURATION)
    {
        TargetUnit = targetUnit;
        UnitActionType = unitActionType;
    }

    /// <summary>
    /// Юнит.
    /// </summary>
    public BattleUnit TargetUnit { get; }

    /// <summary>
    /// Действие юнита.
    /// </summary>
    public UnitActionType UnitActionType { get; }
}