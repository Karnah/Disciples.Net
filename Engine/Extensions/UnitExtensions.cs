using Engine.Enums;
using Engine.Models;

namespace Engine.Extensions
{
    public static class UnitExtensions
    {
        /// <summary>
        /// Проверить, что атака юнита может быть направлена на союзников
        /// </summary>
        public static bool HasAllyAbility(this Unit unit)
        {
            var attackClass = unit.UnitType.FirstAttack.AttackClass;
            if (attackClass == AttackClass.Heal ||
                attackClass == AttackClass.BoostDamage ||
                attackClass == AttackClass.Revive ||
                attackClass == AttackClass.Cure ||
                attackClass == AttackClass.GiveAttack ||
                attackClass == AttackClass.TransformSelf ||
                attackClass == AttackClass.BestowWards)
                return true;

            return false;
        }

        /// <summary>
        /// Проверить, что атака юнита может быть направлена на врагов
        /// </summary>
        public static bool HasEnemyAbility(this Unit unit)
        {
            var attackClass = unit.UnitType.FirstAttack.AttackClass;
            if (attackClass == AttackClass.Damage ||
                attackClass == AttackClass.Drain ||
                attackClass == AttackClass.Paralyze ||
                attackClass == AttackClass.Fear ||
                attackClass == AttackClass.Petrify ||
                attackClass == AttackClass.LowerDamage ||
                attackClass == AttackClass.LowerInitiative ||
                attackClass == AttackClass.Poison ||
                attackClass == AttackClass.Frostbite ||
                attackClass == AttackClass.DrainOverflow ||
                attackClass == AttackClass.DrainLevel ||
                attackClass == AttackClass.Doppelganger ||
                attackClass == AttackClass.TransformOther ||
                attackClass == AttackClass.Blister ||
                attackClass == AttackClass.Shatter)
                return true;

            return false;
        }
    }
}
