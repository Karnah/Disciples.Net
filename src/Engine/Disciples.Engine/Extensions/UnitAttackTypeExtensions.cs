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
            case UnitAttackType.IncreaseDamage:
            case UnitAttackType.Revive:
            case UnitAttackType.Cure:
            case UnitAttackType.Summon:
            case UnitAttackType.GiveAdditionalAttack:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.GiveProtection:
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
            case UnitAttackType.ReduceLevel:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformEnemy:
            case UnitAttackType.Blister:
            case UnitAttackType.ReduceArmor:
                return true;

            default:
                return false;
        }
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