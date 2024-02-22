using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Resources.Enum;

namespace Disciples.Scene.Battle.Resources.ImageKeys.Extensions;

/// <summary>
/// Расширения для работы с ресурсами изображений.
/// </summary>
internal static class ImageResourceKeyExtensions
{
    /// <summary>
    /// Получить ключ в ресурсах, который соответствует определённому действию.
    /// </summary>
    public static string GetResourceKey(this BattleUnitState unitState)
    {
        return unitState switch
        {
            BattleUnitState.Waiting => "IDLE",
            BattleUnitState.Attacking => "HMOV",
            BattleUnitState.TakingDamage => "HHIT",
            BattleUnitState.Paralyzed => "STIL",
            _ => throw new ArgumentOutOfRangeException(nameof(unitState), unitState, null)
        };
    }

    /// <summary>
    /// Получить ключ в ресурсах, которые соответствует определённой части анимации.
    /// </summary>
    public static string GetResourceKey(this UnitAnimationType animationType)
    {
        return animationType switch
        {
            UnitAnimationType.Body => "A1",
            UnitAnimationType.Shadow => "S1",
            UnitAnimationType.Aura => "A2",
            _ => throw new ArgumentOutOfRangeException(nameof(animationType), animationType, null)
        };
    }

    /// <summary>
    /// Получить ключ в ресурсах, который соответствует определённому положению.
    /// </summary>
    public static string GetResourceKey(this BattleDirection direction)
    {
        return direction switch
        {
            BattleDirection.Face => "A",
            BattleDirection.Back => "D",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    /// <summary>
    /// Получить ключ в ресурсах, который соответствует определённому положению отряда.
    /// </summary>
    public static string GetResourceKey(this BattleSquadPosition? squadPosition)
    {
        return squadPosition switch
        {
            BattleSquadPosition.Attacker => "A",
            BattleSquadPosition.Defender => "D",
            null => "B", // Симметричная анимация.
            _ => throw new ArgumentOutOfRangeException(nameof(squadPosition), squadPosition, null)
        };
    }

    /// <summary>
    /// Получить ключ в ресурсах, который соответствует определённой анимации атаки.
    /// </summary>
    public static string GetResourceKey(this UnitTargetAnimationType animationType)
    {
        return animationType switch
        {
            UnitTargetAnimationType.Single => "TUCHA",
            UnitTargetAnimationType.Area => "HEFFA",
            _ => throw new ArgumentOutOfRangeException(nameof(animationType), animationType, null)
        };
    }

    /// <summary>
    /// Получить ключ в ресурсах, который соответствует определённой анимации атаки.
    /// </summary>
    public static string GetResourceKey(this UnitDeathAnimationType animationType)
    {
        return animationType switch
        {
            UnitDeathAnimationType.None => string.Empty,
            UnitDeathAnimationType.Human => "DEATH_HUMAN_S13",
            UnitDeathAnimationType.Heretic => "DEATH_HERETIC_S13",
            UnitDeathAnimationType.Dwarf => "DEATH_DWARF_S15",
            UnitDeathAnimationType.Undead => "DEATH_UNDEAD_S15",
            UnitDeathAnimationType.Neutral => "DEATH_NEUTRAL_S10",
            UnitDeathAnimationType.Dragon => "DEATH_DRAGON_S15",
            UnitDeathAnimationType.Ghost => "DEATH_GHOST_S15",
            UnitDeathAnimationType.Elf => "DEATH_ELF_S15",
            _ => throw new ArgumentOutOfRangeException(nameof(animationType), animationType, null)
        };
    }

    /// <summary>
    /// Проверить, есть ли в ресурсах определённая анимации атаки.
    /// </summary>
    public static bool HasResourceKey(this UnitAttackType attackType)
    {
        return attackType switch
        {
            UnitAttackType.Poison => true,
            UnitAttackType.Frostbite => true,
            UnitAttackType.Revive => true,
            UnitAttackType.TransformEnemy => true,
            UnitAttackType.Blister => true,
            _ => false
        };
    }

    /// <summary>
    /// Получить ключ в ресурсах, который соответствует определённой анимации атаки.
    /// </summary>
    public static string GetResourceKey(this UnitAttackType attackType)
    {
        return attackType switch
        {
            UnitAttackType.Poison => "POISONANIM",
            UnitAttackType.Frostbite => "FROSTBITEANIM",
            UnitAttackType.Revive => "REVIVEANIM",
            UnitAttackType.TransformEnemy => "TRANSF",
            UnitAttackType.Blister => "BLISTERANIM",
            _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
        };
    }

    /// <summary>
    /// Получить ключ в ресурсах, который соответствует определённой анимации атаки.
    /// </summary>
    public static string GetIsSmallResourceKey(this bool isSmall)
    {
        return isSmall
            ? "S"
            : "L";
    }
}