using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Атака юнита.
    /// </summary>
    public class AttackUnitBattleAction : TouchUnitBattleAction
    {
        /// <inheritdoc />
        public AttackUnitBattleAction(BattleUnit targetUnit, int power, AttackClass attackClass) : base(targetUnit, TouchUnitActionType.Attack)
        {
            Power = power;
            AttackClass = attackClass;
        }


        /// <summary>
        /// Сила воздействия.
        /// </summary>
        public int Power { get; }

        /// <summary>
        /// Тип атаки.
        /// </summary>
        public AttackClass AttackClass { get; }
    }
}