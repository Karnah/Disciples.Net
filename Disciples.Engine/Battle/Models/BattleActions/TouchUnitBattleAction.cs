using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.GameObjects;

namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Воздействие на юнита.
    /// </summary>
    public class TouchUnitBattleAction : BaseTimerBattleAction
    {
        /// <summary>
        /// Продолжительность воздействия
        /// </summary>
        private const int TOUCH_UNIT_ACTION_DURATION = 1000;

        /// <inheritdoc />
        public TouchUnitBattleAction(BattleUnit targetUnit, TouchUnitActionType touchUnitActionType) : base(TOUCH_UNIT_ACTION_DURATION)
        {
            TargetUnit = targetUnit;
            TouchUnitActionType = touchUnitActionType;
        }


        /// <summary>
        /// Цель воздействия.
        /// </summary>
        public BattleUnit TargetUnit { get; }

        /// <summary>
        /// Тип воздействия на юнита.
        /// </summary>
        public TouchUnitActionType TouchUnitActionType { get; }
    }
}