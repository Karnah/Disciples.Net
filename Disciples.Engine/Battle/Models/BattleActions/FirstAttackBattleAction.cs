using Disciples.Engine.Battle.GameObjects;

namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Действие атаки одного юнита на другого.
    /// </summary>
    public class FirstAttackBattleAction : AnimationBattleAction
    {
        /// <inheritdoc />
        public FirstAttackBattleAction(BattleUnit attacker, BattleUnit target)
            : base(attacker.AnimationComponent, CalculateEndFrameIndex(attacker))
        {
            Attacker = attacker;
            Target = target;
        }


        /// <summary>
        /// Юнит, который атаковал.
        /// </summary>
        public BattleUnit Attacker { get; }

        /// <summary>
        /// Юнит, который являлся целью атаки.
        /// </summary>
        public BattleUnit Target { get; }


        /// <summary>
        /// Вычислить индекс завершения анимации.
        /// </summary>
        /// <remarks>
        /// Этот метод нужен только потому, что я не смог найти связь между анимациями и моментом нанесения удара.
        /// </remarks>
        private static int CalculateEndFrameIndex(BattleUnit unit)
        {
            var framesCount = unit.AnimationComponent.FramesCount;

            // Ассасин Империи, 41 фрейм.
            if (unit.Unit.UnitType.UnitTypeId == "g000uu0154")
                return 12;

            // Рыцарь, 31 фрейм.
            if (unit.Unit.UnitType.UnitTypeId == "g000uu0002")
                return 12;

            // Страж Горна, 31 фрейм.
            if (unit.Unit.UnitType.UnitTypeId == "g000uu0162")
                return 26;

            // Арбалетчик, 26 фреймов.
            if (unit.Unit.UnitType.UnitTypeId == "g000uu0027")
                return 5;

            // Мастер рун, 30 фреймов.
            if (unit.Unit.UnitType.UnitTypeId == "g000uu0165")
                return 10;


            return framesCount - 12;
        }
    }
}