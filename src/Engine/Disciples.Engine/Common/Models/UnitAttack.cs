using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Атака юнита.
/// </summary>
public class UnitAttack
{
    /// <summary>
    /// Идентификатор атаки.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Наименование атаки.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Описание атаки.
    /// </summary>
    public string Description { get; init; } = null!;

    /// <summary>
    /// Инициатива.
    /// </summary>
    public int Initiative { get; init; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType AttackType { get; init; }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource AttackSource { get; init; }

    /// <summary>
    /// Точность.
    /// </summary>
    public int Accuracy { get; init; }

    /// <summary>
    /// Достижимость целей для атаки.
    /// </summary>
    public UnitAttackReach Reach { get; init; }

    /// <summary>
    /// Сила исцеления.
    /// </summary>
    public int HealPower { get; init; }

    /// <summary>
    /// Наносимый урон.
    /// </summary>
    public int DamagePower { get; init; }

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
    /// Наложение защиты от типов атак.
    /// </summary>
    public IReadOnlyList<UnitAttackTypeProtection> AttackTypeProtections { get; init; } = null!;

    /// <summary>
    /// Наложение защиты от источников атак.
    /// </summary>
    public IReadOnlyList<UnitAttackSourceProtection> AttackSourceProtections { get; init; } = null!;

    /// <summary>
    /// Признак, что при ударе наносится критический урон.
    /// </summary>
    /// <remarks>
    /// +5% от силы атаки.
    /// </remarks>
    public bool IsCritical { get; init; }

    /// <summary>
    /// Призываемые / превращаемые типы юнитов.
    /// </summary>
    /// <remarks>
    /// Для атаки типа <see cref="UnitAttackType.Summon" />, идентификаторы вызываемых юнитов.
    /// Для атак типа <see cref="UnitAttackType.TransformSelf" /> и <see cref="UnitAttackType.TransformEnemy" /> идентификаторы во что идёт превращение.
    /// </remarks>>
    public IReadOnlyList<UnitType> SummonTransformUnitTypes { get; set; } = Array.Empty<UnitType>();
}