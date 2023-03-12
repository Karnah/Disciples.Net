using Disciples.Resources.Database.Sqlite.Enums;

namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Защита юнита от типа атаки.
/// </summary>
public class UnitAttackTypeProtection : IEntity
{
    /// <summary>
    /// Идентификатор юнита, который имеет защиту.
    /// </summary>
    /// <remarks>
    /// Идентификатор НЕ уникален.
    /// </remarks>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Тип юнита, который имеет защиту.
    /// </summary>
    public UnitType UnitType { get; init; } = null!;

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType UnitAttackType { get; init; }

    /// <summary>
    /// Категория защиты.
    /// </summary>
    public ProtectionCategory ProtectionCategory { get; init; }
}