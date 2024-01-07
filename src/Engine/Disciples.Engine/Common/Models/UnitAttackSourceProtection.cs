using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Защита юнита от источника атак.
/// </summary>
public class UnitAttackSourceProtection
{
    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource UnitAttackSource { get; init; }

    /// <summary>
    /// Категория защиты.
    /// </summary>
    public ProtectionCategory ProtectionCategory { get; init; }
}