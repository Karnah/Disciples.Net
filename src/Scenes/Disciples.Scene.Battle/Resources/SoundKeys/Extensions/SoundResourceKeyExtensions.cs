﻿using Disciples.Engine.Common.Enums.Units;

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
            UnitAttackType.ReduceLevel or
            UnitAttackType.Doppelganger or
            UnitAttackType.TransformSelf or
            UnitAttackType.TransformEnemy or
            UnitAttackType.Blister or
            UnitAttackType.ReduceArmor;
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
            UnitAttackType.ReduceLevel => "UNTRANSF",
            UnitAttackType.Doppelganger => "UNTRANSF",
            UnitAttackType.TransformSelf => "UNTRANSF",
            UnitAttackType.TransformEnemy => "UNTRANSF",
            UnitAttackType.Blister => "BLISTER",
            UnitAttackType.ReduceArmor => "SHATTER",
            _ => throw new ArgumentOutOfRangeException(nameof(unitAttackType), unitAttackType, null)
        };
    }
}