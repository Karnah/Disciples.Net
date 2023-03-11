using Disciples.Resources.Database.Sqlite.Components;
using Disciples.Resources.Database.Sqlite.Enums;

namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Раса.
/// </summary>
public class Race : IEntity
{
    /// <summary>
    /// Идентификатор расы.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Идентификатор юнита-стража столицы.
    /// </summary>
    public UnitType? GuardingUnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор лидера-вора расы.
    /// </summary>
    public UnitType? LeaderThiefUnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор первого героя расы.
    /// </summary>
    public UnitType? Leader1UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор второго героя расы.
    /// </summary>
    public UnitType? Leader2UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор третьего героя расы.
    /// </summary>
    public UnitType? Leader3UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор четвертого героя расы.
    /// </summary>
    public UnitType? Leader4UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор первого солдата расы.
    /// </summary>
    public UnitType? Soldier1UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор второго солдата расы.
    /// </summary>
    public UnitType? Soldier2UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор третьего солдата расы.
    /// </summary>
    public UnitType? Soldier3UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор четвертого солдата расы.
    /// </summary>
    public UnitType? Soldier4UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор пятого солдата расы.
    /// </summary>
    public UnitType? Soldier5UnitTypeId { get; init; }

    /// <summary>
    /// Дальность обзора столицы вокруг себя.
    /// </summary>
    public int CapitalScoutPoints { get; init; }

    /// <summary>
    /// Восстановление % жизней за ход в столице.
    /// </summary>
    public int CapitalHitPointsRegeneration { get; init; }

    /// <summary>
    /// Доход в ресурсах каждый ход.
    /// </summary>
    public ResourceSet Income { get; init; } = null!;

    /// <summary>
    /// Идентификатор модификатора, который устанавливает бонус к защите юнитов в столице.
    /// </summary>
    public string? CapitalProtectionModifierId { get; init; }

    /// <summary>
    /// Наименование расы.
    /// </summary>
    public InterfaceTextResource Name { get; init; } = null!;

    /// <summary>
    /// Тип расы.
    /// </summary>
    public RaceType RaceType { get; init; }

    /// <summary>
    /// Можно ли играть данной расой.
    /// </summary>
    public bool IsPlayable { get; init; }
}