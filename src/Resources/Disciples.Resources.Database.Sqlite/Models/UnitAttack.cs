using Disciples.Resources.Database.Sqlite.Enums;

namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Атака юнита.
/// </summary>
public class UnitAttack : IEntity
{
    /// <summary>
    /// Идентификатор атаки.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Название атаки.
    /// </summary>
    public GlobalTextResource Name { get; init; } = null!;

    /// <summary>
    /// Описание атаки.
    /// </summary>
    public GlobalTextResource Description { get; init; } = null!;

    /// <summary>
    /// Инициатива.
    /// </summary>
    public int Initiative { get; init; }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource AttackSource { get; init; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType AttackType { get; init; }

    /// <summary>
    /// Точность.
    /// </summary>
    public int Accuracy { get; init; }

    /// <summary>
    /// Достижимость целей для атаки.
    /// </summary>
    public UnitAttackReach Reach { get; init; }

    /// <summary>
    /// Наносимый урон.
    /// </summary>
    public int DamagePower { get; init; }

    /// <summary>
    /// Сила исцеления.
    /// </summary>
    public int HealPower { get; init; }

    /// <summary>
    /// Уровень атаки.
    /// </summary>
    /// <remarks>
    /// Используется, например, для ветки травниц Горных Кланов.
    /// Уровень показывается на сколько будет усилена атака - 25%/50%/100%.
    /// </remarks>
    public int AttackPowerLevel { get; init; }

    /// <summary>
    /// Идентификатор альтернативной атаки.
    /// </summary>
    /// <remarks>
    /// Используется для доппельгангера и повелителя волков.
    /// Основной атакой они превращаются, альтернативной бьют врагов.
    /// </remarks>
    public UnitAttack? AlternativeAttack { get; init; }

    /// <summary>
    /// Признак, что эффект, накладываемый атакой, длится до конца боя.
    /// </summary>
    public bool IsInfinitive { get; init; }

    /// <summary>
    /// Количество защит, которые накладывается на цель при атаке.
    /// </summary>
    /// <remarks>
    /// В зависимости от этого значения берётся <see cref="Ward1" />, <see cref="Ward2" />, <see cref="Ward3" /> и <see cref="Ward4" />.
    /// </remarks>
    public int WardsCount { get; init; }

    /// <summary>
    /// Тип первой защиты, которая накладывается при атаке.
    /// </summary>
    public UnitModifier? Ward1 { get; init; }

    /// <summary>
    /// Тип второй защиты, которая накладывается при атаке.
    /// </summary>
    public UnitModifier? Ward2 { get; init; }

    /// <summary>
    /// Тип третьей защиты, которая накладывается при атаке.
    /// </summary>
    public UnitModifier? Ward3 { get; init; }

    /// <summary>
    /// Тип четвертой защиты, которая накладывается при атаке.
    /// </summary>
    public UnitModifier? Ward4 { get; init; }

    /// <summary>
    /// Признак, что при ударе наносится критический урон.
    /// </summary>
    /// <remarks>
    /// +5% от силы атаки.
    /// </remarks>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Для атаки типа <see cref="UnitAttackType.Summon" />, идентификаторы вызываемых юнитов.
    /// Для атак типа <see cref="UnitAttackType.TransformSelf" /> и <see cref="UnitAttackType.TransformOther" /> идентификаторы во что идёт превращение.
    /// </summary>
    public ICollection<UnitAttackSummonTransform> AttackSummonTransforms { get; init; } = null!;
}