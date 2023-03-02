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
        var attackClass = unit.UnitType.MainAttack.AttackClass;
        if (attackClass is AttackClass.Heal
            or AttackClass.BoostDamage
            or AttackClass.Revive
            or AttackClass.Cure
            or AttackClass.GiveAttack
            or AttackClass.TransformSelf
            or AttackClass.BestowWards)
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
        var attackClass = unit.UnitType.MainAttack.AttackClass;
        if (attackClass is AttackClass.Damage
            or AttackClass.Drain
            or AttackClass.Paralyze
            or AttackClass.Fear
            or AttackClass.Petrify
            or AttackClass.LowerDamage
            or AttackClass.LowerInitiative
            or AttackClass.Poison
            or AttackClass.Frostbite
            or AttackClass.DrainOverflow
            or AttackClass.DrainLevel
            or AttackClass.Doppelganger
            or AttackClass.TransformOther
            or AttackClass.Blister
            or AttackClass.Shatter)
        {
            return true;
        }

        return false;
    }
}