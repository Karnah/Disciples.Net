﻿using Disciples.Engine.Battle.Components;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Battle.GameObjects
{
    /// <summary>
    /// Игровой объект юнита во время сражения.
    /// </summary>
    public class BattleUnit : GameObject
    {
        /// <summary>
        /// Ширина юнита на сцене.
        /// </summary>
        private const int BATTLE_UNIT_WIDTH = 75;
        /// <summary>
        /// Высота юнита на сцене.
        /// </summary>
        private const int BATTLE_UNIT_HEIGHT = 100;

        public BattleUnit(
            IVisualSceneController visualSceneController,
            IBattleUnitResourceProvider battleUnitResourceProvider,
            Unit unit,
            bool isAttacker
            ) : base(GetSceneUnitPosition(isAttacker, unit.SquadLinePosition, unit.SquadFlankPosition))
        {
            Unit = unit;
            IsAttacker = isAttacker;
            Direction = isAttacker
                ? BattleDirection.Attacker
                : BattleDirection.Defender;
            Action = BattleAction.Waiting;

            BattleUnitAnimationComponent = new BattleUnitAnimationComponent(this, visualSceneController, battleUnitResourceProvider, unit.UnitType.UnitTypeId);
            this.Components = new IComponent[] { BattleUnitAnimationComponent };

            Width = BATTLE_UNIT_WIDTH;
            Height = BATTLE_UNIT_HEIGHT;
        }


        /// <inheritdoc />
        public override bool IsInteractive => true;

        /// <summary>
        /// Компонент анимации юнита.
        /// </summary>
        public BattleUnitAnimationComponent BattleUnitAnimationComponent { get; }


        /// <summary>
        /// Информация о юните.
        /// </summary>
        public Unit Unit { get; }

        /// <summary>
        /// Принадлежит ли юнит атакующему отряду.
        /// </summary>
        public bool IsAttacker { get; }

        /// <summary>
        /// Направление, куда смотрит юнит.
        /// </summary>
        public BattleDirection Direction { get; set; }

        /// <summary>
        /// Действие, которое выполняет юнит в данный момент.
        /// </summary>
        public BattleAction Action { get; set; }


        /// <summary>
        /// Рассчитать позицию юнита на сцене.
        /// </summary>
        /// <param name="isAttacker">Находится ли юнит в атакующем отряде.</param>
        /// <param name="line">Линия, на которой располагается юнит.</param>
        /// <param name="flank">Позиция на которой находится юнит (центр, правый и левый фланги).</param>
        public static (double X, double Y) GetSceneUnitPosition(bool isAttacker, double line, double flank)
        {
            // Если смотреть на поле, то фронт защищающегося отряда (линия 0) - это 2 линия
            // Тыл же (линия 1) будет на 3 линии. Поэтому пересчитываем положение
            var gameLine = isAttacker
                ? line
                : 3 - line;

            var x = 60 + 95 * gameLine + 123 * flank;
            var y = 200 + 60 * gameLine - 43 * flank;

            return (x, y);
        }
    }
}