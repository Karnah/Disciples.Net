using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Scene.Battle.Resources.SoundKeys.Extensions;

/// <summary>
/// Расширения для работы с ресурсами звуков.
/// </summary>
internal static class SoundResourceKeyExtensions
{
    /// <summary>
    /// Проверить, есть ли ключ в ресурсах, который соответствует определённому действию.
    /// </summary>
    public static bool HasResourceKey(this UnitAttackType unitAttackType)
    {
        return unitAttackType is UnitAttackType.Heal or
            UnitAttackType.Poison or
            UnitAttackType.Frostbite or
            UnitAttackType.Blister or
            UnitAttackType.Shatter;
    }

    /// <summary>
    /// Получить ключ в ресурсах, который соответствует определённому действию.
    /// </summary>
    public static string GetResourceKey(this UnitAttackType unitAttackType)
    {
        return unitAttackType switch
        {
            UnitAttackType.Heal => "HEAL",
            UnitAttackType.Poison => "POISON",
            UnitAttackType.Frostbite => "FRSTBITE",
            UnitAttackType.Blister => "BLISTER",
            UnitAttackType.Shatter => "SHATTER",
            _ => throw new ArgumentOutOfRangeException(nameof(unitAttackType), unitAttackType, null)
        };
    }
}