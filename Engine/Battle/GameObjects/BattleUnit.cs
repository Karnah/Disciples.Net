using System.Collections.Generic;

using Engine.Battle.Components;
using Engine.Battle.Enums;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Common.Components;
using Engine.Common.Controllers;
using Engine.Common.GameObjects;
using Engine.Common.Models;

namespace Engine.Battle.GameObjects
{
    public class BattleUnit : GameObject
    {
        public BattleUnit(IMapVisual mapVisual, IBattleUnitResourceProvider battleUnitResourceProvider, Unit unit, bool isAttacker)
            : base(GetSceneUnitPosition(isAttacker, unit.SquadLinePosition, unit.SquadFlankPosition))
        {
            Unit = unit;
            IsAttacker = isAttacker;
            Direction = isAttacker
                ? BattleDirection.Attacker
                : BattleDirection.Defender;
            Action = BattleAction.Waiting;
            BattleUnitEffects = new HashSet<BattleUnitEffect>();

            BattleUnitAnimationComponent = new BattleUnitAnimationComponent(this, mapVisual, battleUnitResourceProvider, unit.UnitType.UnitTypeId);
            this.Components = new IComponent[] { BattleUnitAnimationComponent };
        }


        public BattleUnitAnimationComponent BattleUnitAnimationComponent { get; }


        public Unit Unit { get; }

        public bool IsAttacker { get; }

        public BattleDirection Direction { get; set; }

        public BattleAction Action { get; set; }

        public HashSet<BattleUnitEffect> BattleUnitEffects { get; }


        /// <summary>
        /// Рассчитать позицию юнита на сцене
        /// </summary>
        /// <param name="isAttacker">Находится ли юнит в атакующем отряде</param>
        /// <param name="line">Линия, на которой распологается юнит</param>
        /// <param name="flank">Позиция на которой находится юнит (центр, правый и левый фланги)</param>
        public static (double X, double Y) GetSceneUnitPosition(bool isAttacker, double line, double flank)
        {
            // Если смотреть на поле, то фронт защищающегося отряда (линия 0) - это 2 линия
            // Тыл же (линия 1) будет на 3 линии. Поэтому пересчитываем положение
            var gameLine = isAttacker
                ? line
                : 3 - line;

            var x = -340 + 95 * gameLine + 123 * flank;
            var y = -230 + 60 * gameLine - 43 * flank;

            return (x, y);
        }
    }
}
