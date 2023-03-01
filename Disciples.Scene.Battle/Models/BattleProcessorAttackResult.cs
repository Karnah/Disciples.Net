using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models
{
    /// <summary>
    /// Результат атаки одного юнита на другого.
    /// </summary>
    public class BattleProcessorAttackResult
    {
        public BattleProcessorAttackResult(AttackResult attackResult)
        {
            AttackResult = attackResult;
        }

        public BattleProcessorAttackResult(AttackResult attackResult, int power, AttackClass attackClass)
        {
            AttackResult = attackResult;
            Power = power;
            AttackClass = attackClass;
        }

        public BattleProcessorAttackResult(AttackResult attackResult, int? power, int roundDuration, AttackClass attackClass)
        {
            AttackResult = attackResult;
            Power = power;
            RoundDuration = roundDuration;
            AttackClass = attackClass;
        }

        public AttackResult AttackResult { get; }

        /// <summary>
        /// Сила воздействия.
        /// </summary>
        public int? Power { get; }

        /// <summary>
        /// Длительность эффекта в раундах.
        /// </summary>
        public int? RoundDuration { get; set; }

        /// <summary>
        /// Тип атаки.
        /// </summary>
        public AttackClass? AttackClass { get; }
    }
}
