using System;
using System.Linq;

using Engine.Enums;
using Engine.Extensions;
using Engine.Interfaces;

using Action = System.Action;

namespace Engine.Implementation.Game
{
    public class BattleAttackController : IBattleAttackController
    {
        public bool AttackUnit(
            BattleUnit currentUnitGameObject,
            BattleUnit[] currentSquad,
            BattleUnit targetUnitGameObject,
            BattleUnit[] targetSquad,
            Action onAttackEnd)
        {
            var currentUnit = currentUnitGameObject.Unit;
            var targetUnit = targetUnitGameObject.Unit;

            // Лекарь не может атаковать врага, а воин не может атаковать союзника
            if (currentUnit.Player == targetUnit.Player && currentUnit.HasAllyAbility() == false ||
                currentUnit.Player != targetUnit.Player && currentUnit.HasEnemyAbility() == false) {
                return false;
            }

            // Если юнит может атаковать только ближайшего, то проверяем препятствия
            if (currentUnit.UnitType.FirstAttack.Reach == Reach.Adjacent) {
                // Проверка, может ли юнит дотянуть до врага, несмотря на возможное наличие линий между ними
                if (CanAttackOnFlank(
                        currentUnit.SquadFlankPosition,
                        targetUnit.SquadFlankPosition,
                        targetUnit.SquadLinePosition,
                        targetSquad) == false)
                    return false;

                // Если атакующий юнит находится сзади и есть линия союзников впереди
                if (currentUnit.SquadLinePosition == 0 && IsFirstLineEmpty(currentSquad) == false)
                    return false;

                // Если враг находится сзади, то проверяем, что нет первой вражеской линии
                if (targetUnit.SquadLinePosition == 0 && IsFirstLineEmpty(targetSquad) == false)
                    return false;
            }

            var battleUnits = currentUnit.UnitType.FirstAttack.Reach == Reach.All
                ? targetSquad :
                new [] { targetUnitGameObject };

            currentUnitGameObject.AttackUnits(battleUnits, onAttackEnd);

            return true;
        }


        /// <summary>
        /// Проверить, свободна ли первая линия в отряде
        /// </summary>
        private static bool IsFirstLineEmpty(BattleUnit[] squad)
        {
            return squad.Any(u => u.Unit.SquadLinePosition == 1) == false;
        }

        /// <summary>
        /// Проверить, можно ли атаковать цель в зависимости от расположения на фланге
        /// </summary>
        private static bool CanAttackOnFlank(int currentUnitFlankPosition, int targetUnitFlankPosition, int targetUnitLinePosition, BattleUnit[] targetSquad)
        {
            // Если юниты находятся по разные стороны флагов и занят вражеский центр или соседняя с атакующим клетка, то атаковать нельзя
            if (Math.Abs(currentUnitFlankPosition - targetUnitFlankPosition) > 1 &&
                (IsPlaceEmpty(targetSquad, targetUnitLinePosition, 1) == false || IsPlaceEmpty(targetSquad, targetUnitLinePosition, currentUnitFlankPosition) == false))
                return false;

            return true;
        }

        /// <summary>
        /// Проверить, свобдна ли клетка на арене
        /// </summary>
        private static bool IsPlaceEmpty(BattleUnit[] squad, int line, int flank)
        {
            return squad.Any(u => u.Unit.SquadLinePosition == line && u.Unit.SquadFlankPosition == flank) == false;
        }
    }
}
