using Disciples.Resources.Database.Sqlite.Enums;

namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Защита юнита от источника атак.
/// </summary>
public class UnitAttackSourceProtection : IEntity
{
    /// <summary>
    /// Идентификатор юнита, который имеет защиту.
    /// </summary>
    /// <remarks>
    /// Идентификатор НЕ уникален.
    /// </remarks>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource UnitAttackSource { get; init; }

    /// <summary>
    /// Категория защиты.
    /// </summary>
    public ProtectionCategory ProtectionCategory { get; init; }
}