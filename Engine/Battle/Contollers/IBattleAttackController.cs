using System;
using System.Collections.Generic;

using Engine.Battle.Enums;
using Engine.Battle.GameObjects;

namespace Engine.Battle.Contollers
{
    public interface IBattleAttackController
    {
        /// <summary>
        /// Текущее состояние битвы
        /// </summary>
        BattleState BattleState { get; }

        /// <summary>
        /// Юнит, который сейчас ходит
        /// </summary>
        BattleUnit CurrentUnitObject { get; }

        /// <summary>
        /// Все юниты
        /// </summary>
        IReadOnlyList<BattleUnit> Units { get; }


        /// <summary>
        /// Событие возникает, когда юнит начинает действие
        /// </summary>
        event EventHandler UnitActionBegin;

        /// <summary>
        /// Событие возникает, когда следующий юнит готов к ходу
        /// </summary>
        event EventHandler UnitActionEnded;

        /// <summary>
        /// Событие возникает, когда один из отрядов полностью уничтожен
        /// </summary>
        event EventHandler BattleEnded;


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

        /// <summary>
        /// Защититься на этом ходу
        /// </summary>
        void Defend();

        /// <summary>
        /// Подождать
        /// </summary>
        void Wait();
    }
}
