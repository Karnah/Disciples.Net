using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Extensions;

/// <summary>
/// Набор методов для упрощения работы с юнитами.
/// </summary>
public static class UnitExtensions
{
    /// <summary>
    /// Проверить, что атака юнита может быть направлена на союзников.
    /// </summary>
    public static bool HasAllyAbility(this Unit unit)
    {
        var attackClass = unit.UnitType.MainAttack.AttackType;
        if (attackClass is UnitAttackType.Heal
            or UnitAttackType.BoostDamage
            or UnitAttackType.Revive
            or UnitAttackType.Cure
            or UnitAttackType.GiveAdditionalAttack
            or UnitAttackType.TransformSelf
            or UnitAttackType.BestowWards)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Проверить, что атака юнита может быть направлена на врагов.
    /// </summary>
    public static bool HasEnemyAbility(this Unit unit)
    {
        var attackClass = unit.UnitType.MainAttack.AttackType;
        if (attackClass is UnitAttackType.Damage
            or UnitAttackType.DrainLife
            or UnitAttackType.Paralyze
            or UnitAttackType.Fear
            or UnitAttackType.Petrify
            or UnitAttackType.ReduceDamage
            or UnitAttackType.ReduceInitiative
            or UnitAttackType.Poison
            or UnitAttackType.Frostbite
            or UnitAttackType.DrainLifeOverflow
            or UnitAttackType.DrainLevel
            or UnitAttackType.Doppelganger
            or UnitAttackType.TransformOther
            or UnitAttackType.Blister
            or UnitAttackType.Shatter)
        {
            return true;
        }

        return false;
    }
}