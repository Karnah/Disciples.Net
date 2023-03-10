using System.ComponentModel.DataAnnotations.Schema;
using Disciples.Resources.Database.Components;
using Disciples.Resources.Database.Enums;

namespace Disciples.Resources.Database.Models;

/// <summary>
/// Раса.
/// </summary>
[Table("Grace")]
public class Race : IEntity
{
    /// <summary>
    /// Идентификатор расы.
    /// </summary>
    [Column("RACE_ID")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Идентификатор юнита-стража столицы.
    /// </summary>
    [Column("GUARDIAN")]
    public string? GuardingUnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор лидера-вора расы.
    /// </summary>
    [Column("NOBLE")]
    public string? LeaderThiefUnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор первого героя расы.
    /// </summary>
    [Column("LEADER_1")]
    public string? Leader1UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор второго героя расы.
    /// </summary>
    [Column("LEADER_2")]
    public string? Leader2UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор третьего героя расы.
    /// </summary>
    [Column("LEADER_3")]
    public string? Leader3UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор четвертого героя расы.
    /// </summary>
    [Column("LEADER_4")]
    public string? Leader4UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор первого обычного юнита расы.
    /// </summary>
    [Column("SOLDIER_1")]
    public string? Soldier1UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор второго обычного юнита расы.
    /// </summary>
    [Column("SOLDIER_2")]
    public string? Soldier2UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор третьего обычного юнита расы.
    /// </summary>
    [Column("SOLDIER_3")]
    public string? Soldier3UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор четвертого обычного юнита расы.
    /// </summary>
    [Column("SOLDIER_4")]
    public string? Soldier4UnitTypeId { get; init; }

    /// <summary>
    /// Идентификатор пятого обычного юнита расы.
    /// </summary>
    [Column("SOLDIER_5")]
    public string? Soldier5UnitTypeId { get; init; }

    /// <summary>
    /// Дальность обзора столицы вокруг себя.
    /// </summary>
    [Column("SCOUT")]
    public int? CapitalScoutPoints { get; init; }

    /// <summary>
    /// Базовое восстановления % жизней за ход в столице.
    /// </summary>
    [Column("REGEN_H")]
    public int CapitalHitPointsRegeneration { get; init; }

    /// <summary>
    /// Доход в ресурсах каждый ход.
    /// </summary>
    [Column("INCOME")]
    public ResourceCost Income { get; init; } = null!;

    /// <summary>
    /// Идентификатор модификатора, который устанавливает бонус к защите юнитов в столице.
    /// </summary>
    [Column("PROTECTION")]
    public string? CapitalProtectionModifierId { get; init; }

    /// <summary>
    /// Наименование расы.
    /// </summary>
    [Column("NAME_TXT")]
    public string NameTextId { get; init; } = null!;

    /// <summary>
    /// Тип расы.
    /// </summary>
    [Column("RACE_TYPE")]
    public RaceType RaceType { get; init; }

    /// <summary>
    /// Можно ли играть данной расой.
    /// </summary>
    [Column("PLAYABLE")]
    public bool IsPlayable { get; init; }
}