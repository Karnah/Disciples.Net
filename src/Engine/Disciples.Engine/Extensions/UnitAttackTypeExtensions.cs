using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Extensions;

/// <summary>
/// Расширения для <see cref="UnitAttackType" />
/// </summary>
public static class UnitAttackTypeExtensions
{
    /// <summary>
    /// Проверить, что тип атаки направлен на союзников.
    /// </summary>
    public static bool IsAllyAttack(this UnitAttackType unitAttackType)
    {
        switch (unitAttackType)
        {
            case UnitAttackType.Heal:
            case UnitAttackType.BoostDamage:
            case UnitAttackType.Revive:
            case UnitAttackType.Cure:
            case UnitAttackType.GiveAdditionalAttack:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.BestowWards:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Проверить, что тип атаки направлен на врагов.
    /// </summary>
    public static bool IsEnemyAttack(this UnitAttackType unitAttackType)
    {
        switch (unitAttackType)
        {
            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.Paralyze:
            case UnitAttackType.Fear:
            case UnitAttackType.Petrify:
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.DrainLifeOverflow:
            case UnitAttackType.DrainLevel:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformOther:
            case UnitAttackType.Blister:
            case UnitAttackType.Shatter:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Признак, что атака накладывается какой-то эффект.
    /// </summary>
    public static bool IsEffect(this UnitAttackType unitAttackType)
    {
        switch (unitAttackType)
        {
            case UnitAttackType.Paralyze:
            case UnitAttackType.Petrify:
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.Blister:
            case UnitAttackType.BoostDamage:
            case UnitAttackType.DrainLevel:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.TransformOther:
            case UnitAttackType.BestowWards:
            case UnitAttackType.Shatter:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Признак, что атака накладывается эффект, который наносит урон со временем.
    /// </summary>
    public static bool IsDamageEffect(this UnitAttackType unitAttackType)
    {
        return unitAttackType switch
        {
            UnitAttackType.Poison => true,
            UnitAttackType.Frostbite => true,
            UnitAttackType.Blister => true,
            _ => false
        };
    }

    /// <summary>
    /// Признак, что атака наносит прямой урон.
    /// </summary>
    public static bool IsDirectDamage(this UnitAttackType unitAttackType)
    {
        return unitAttackType switch
        {
            UnitAttackType.Damage => true,
            UnitAttackType.DrainLife => true,
            UnitAttackType.DrainLifeOverflow => true,
            _ => false
        };
    }
}