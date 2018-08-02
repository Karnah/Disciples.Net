using System;
using System.Collections.Generic;

namespace Engine.Interfaces
{
    public interface IBattleAttackController
    {
        BattleUnit CurrentUnitGameObject { get; }

        IReadOnlyList<BattleUnit> Units { get; }



        /// <summary>
        /// Проверить на возможность атаки юнита
        /// </summary>
        bool CanAttack(BattleUnit currentUnitGameObject);


        /// <summary>
        /// Атаковать выбранного юнита
        /// </summary>
        /// <param name="targetUnitGameObject">Юнит, являющийся целью</param>
        /// <param name="onAttackEnd">Метод, который будет вызван после завершения анимации атаки</param>
        /// <returns>True, если атака возможна, false если невозможно атаковать</returns>
        bool AttackUnit(BattleUnit targetUnitGameObject, Action onAttackEnd);
    }
}
