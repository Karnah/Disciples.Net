using System.ComponentModel.DataAnnotations.Schema;
using Disciples.Resources.Database.Dbf.Enums;

namespace Disciples.Resources.Database.Dbf.Models;

/// <summary>
/// Атака юнита.
/// </summary>
[Table("Gattacks")]
public class UnitAttack : IEntity
{
    /// <summary>
    /// Идентификатор атаки.
    /// </summary>
    [Column("ATT_ID")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Название атаки.
    /// </summary>
    [Column("NAME_TXT")]
    public string NameTextId { get; init; } = null!;

    /// <summary>
    /// Описание атаки.
    /// </summary>
    [Column("DESC_TXT")]
    public string DescriptionTextId { get; init; } = null!;

    /// <summary>
    /// Инициатива.
    /// </summary>
    [Column("INITIATIVE")]
    public int Initiative { get; init; }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    [Column("SOURCE")]
    public UnitAttackSource AttackSource { get; init; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    [Column("CLASS")]
    public UnitAttackType AttackType { get; init; }

    /// <summary>
    /// Точность.
    /// </summary>
    [Column("POWER")]
    public int Accuracy { get; init; }

    /// <summary>
    /// Достижимость целей для атаки.
    /// </summary>
    [Column("REACH")]
    public UnitAttackReach Reach { get; init; }

    /// <summary>
    /// Сила исцеления.
    /// </summary>
    [Column("QTY_HEAL")]
    public int HealPower { get; init; }

    /// <summary>
    /// Наносимый урон.
    /// </summary>
    [Column("QTY_DAM")]
    public int DamagePower { get; init; }

    /// <summary>
    /// Уровень атаки.
    /// </summary>
    /// <remarks>
    /// Используется, например, для ветки травниц Горных Кланов.
    /// Уровень показывается на сколько будет усилена атака - 25%/50%/100%.
    /// </remarks>
    [Column("LEVEL")]
    public int AttackPowerLevel { get; init; }

    /// <summary>
    /// Идентификатор альтернативной атаки.
    /// </summary>
    /// <remarks>
    /// Используется для доппельгангера и повелителя волков.
    /// Основной атакой они превращаются, альтернативной бьют врагов.
    /// </remarks>
    [Column("ALT_ATTACK")]
    public string? AlternativeUnitAttackId { get; init; }

    /// <summary>
    /// Признак, что эффект, накладываемый атакой, длится до конца боя.
    /// </summary>
    [Column("INFINITE")]
    public bool IsInfinitive { get; init; }

    /// <summary>
    /// Количество защит, которые накладывается на цель при атаке.
    /// </summary>
    /// <remarks>
    /// В зависимости от этого значения берётся <see cref="Ward1Id" />, <see cref="Ward2Id" />, <see cref="Ward3Id" /> и <see cref="Ward4Id" />.
    /// </remarks>
    [Column("QTY_WARDS")]
    public int WardsCount { get; init; }

    /// <summary>
    /// Тип первой защиты, которая накладывается при атаке.
    /// </summary>
    [Column("WARD1")]
    public string? Ward1Id { get; init; }

    /// <summary>
    /// Тип второй защиты, которая накладывается при атаке.
    /// </summary>
    [Column("WARD2")]
    public string? Ward2Id { get; init; }

    /// <summary>
    /// Тип третьей защиты, которая накладывается при атаке.
    /// </summary>
    [Column("WARD3")]
    public string? Ward3Id { get; init; }

    /// <summary>
    /// Тип четвертой защиты, которая накладывается при атаке.
    /// </summary>
    [Column("WARD4")]
    public string? Ward4Id { get; init; }

    /// <summary>
    /// Признак, что при ударе наносится критический урон.
    /// </summary>
    /// <remarks>
    /// +5% от силы атаки.
    /// </remarks>
    [Column("CRIT_HIT")]
    public bool IsCritical { get; init; }
}