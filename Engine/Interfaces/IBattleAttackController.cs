using System;
using System.Collections.Generic;

namespace Engine.Interfaces
{
    public interface IBattleAttackController
    {
        /// <summary>
        /// Юнит, который сейчас ходит
        /// </summary>
        BattleUnit CurrentUnitGameObject { get; }

        /// <summary>
        /// Все юниты
        /// </summary>
        IReadOnlyList<BattleUnit> Units { get; }


        /// <summary>
        /// Событие возникает, когда следующий юнит готов к ходу
        /// </summary>
        event EventHandler AttackEnded;



        /// <summary>
        /// Проверить на возможность атаки юнита
        /// </summary>
        bool CanAttack(BattleUnit currentUnitGameObject);


        /// <summary>
        /// Атаковать выбранного юнита
        /// </summary>
        /// <param name="targetUnitGameObject">Юнит, являющийся целью</param>
        /// <returns>True, если атака возможна, false если невозможно атаковать</returns>
        bool AttackUnit(BattleUnit targetUnitGameObject);
    }
}
