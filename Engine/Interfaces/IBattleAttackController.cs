using System;

namespace Engine.Interfaces
{
    public interface IBattleAttackController
    {
        /// <summary>
        /// Атаковать выбранного юнита
        /// </summary>
        /// <param name="currentUnitGameObject">Юнит, который будет атаковать</param>
        /// <param name="currentSquad">Отряд атакующего юнита</param>
        /// <param name="targetUnitGameObject">Юнит, являющийся целью</param>
        /// <param name="targetSquad">Отряд юнита, являющийся целью</param>
        /// <param name="onAttackEnd">Метод, который будет вызван после завершения анимации атаки</param>
        /// <returns>True, если атака возможна, false если невозможно атаковать</returns>
        bool AttackUnit(BattleUnit currentUnitGameObject, BattleUnit[] currentSquad, BattleUnit targetUnitGameObject, BattleUnit[] targetSquad, Action onAttackEnd);
    }
}
