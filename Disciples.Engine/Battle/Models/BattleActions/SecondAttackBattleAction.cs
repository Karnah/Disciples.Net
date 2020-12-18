using Disciples.Engine.Battle.GameObjects;

namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Действие второй атаки юнита.
    /// </summary>
    public class SecondAttackBattleAction : WaitAction
    {
        /// <inheritdoc />
        public SecondAttackBattleAction(BattleUnit attacker, BattleUnit target)
        {
            Attacker = attacker;
            Target = target;
        }

        /// <inheritdoc />
        public SecondAttackBattleAction(BattleUnit attacker, BattleUnit target, int power)
        {
            Attacker = attacker;
            Target = target;
            Power = power;
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
        /// Сила атаки.
        /// </summary>
        /// <remarks>
        /// Нужна в тех случаях, когда сила зависит от первой атаки. Например, если восстанавливаем здоровье за счёт урона.
        /// </remarks>
        public int? Power { get; }
    }
}