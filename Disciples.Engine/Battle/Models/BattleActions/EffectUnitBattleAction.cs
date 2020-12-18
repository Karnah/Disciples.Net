using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Наложение эффекта на юнита.
    /// </summary>
    public class EffectUnitBattleAction : TouchUnitBattleAction
    {
        /// <inheritdoc />
        public EffectUnitBattleAction(BattleUnit targetUnit, AttackClass attackClass) : base(targetUnit, TouchUnitActionType.Effect)
        {
            AttackClass = attackClass;
        }

        /// <inheritdoc />
        public EffectUnitBattleAction(BattleUnit targetUnit, AttackClass attackClass, int roundDuration, int? power)
            : base(targetUnit, TouchUnitActionType.Effect)
        {
            AttackClass = attackClass;
            RoundDuration = roundDuration;
            Power = power;
        }


        /// <summary>
        /// Атака из-за которой был наложен эффект.
        /// </summary>
        public AttackClass AttackClass { get; }

        /// <summary>
        /// Продолжительность эффекта в раундах.
        /// </summary>
        public int RoundDuration { get; }

        /// <summary>
        /// Сила эффекта.
        /// </summary>
        public int? Power { get; }
    }
}