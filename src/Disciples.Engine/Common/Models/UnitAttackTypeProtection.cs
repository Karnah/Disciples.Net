using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Защита юнита от типа атаки.
/// </summary>
public class UnitAttackTypeProtection
{
    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackType UnitAttackType { get; init; }

    /// <summary>
    /// Категория защиты.
    /// </summary>
    public ProtectionCategory ProtectionCategory { get; init; }
}