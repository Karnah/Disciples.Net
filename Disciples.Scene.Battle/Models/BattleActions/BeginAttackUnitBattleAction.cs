using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions
{
    /// <summary>
    /// Событие начала атаки указанного юнита.
    /// </summary>
    public class BeginAttackUnitBattleAction : BaseEventBattleAction
    {
        /// <summary>
        /// Создать объект типа <see cref="BeginAttackUnitBattleAction" />.
        /// </summary>
        /// <param name="targetBattleUnit">Цель воздействия.</param>
        public BeginAttackUnitBattleAction(BattleUnit targetBattleUnit)
        {
            TargetBattleUnit = targetBattleUnit;
        }

        /// <summary>
        /// Цель воздействия.
        /// </summary>
        public BattleUnit TargetBattleUnit { get; }
    }
}
