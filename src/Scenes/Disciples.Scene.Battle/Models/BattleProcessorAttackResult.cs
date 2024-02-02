using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Результат атаки одного юнита на другого.
/// </summary>
internal class BattleProcessorAttackResult
{
    /// <summary>
    /// Создать объект типа <see cref="BattleProcessorAttackResult" />.
    /// </summary>
    public BattleProcessorAttackResult(AttackResult attackResult)
    {
        AttackResult = attackResult;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleProcessorAttackResult" />.
    /// </summary>
    public BattleProcessorAttackResult(AttackResult attackResult, UnitAttackType attackType, UnitAttackSource attackSource)
    {
        AttackResult = attackResult;
        AttackType = attackType;
        AttackSource = attackSource;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleProcessorAttackResult" /> для прямой атаки юнита.
    /// </summary>
    public BattleProcessorAttackResult(AttackResult attackResult, UnitAttackType attackType, UnitAttackSource attackSource, int power, int? criticalDamage = null)
    {
        AttackResult = attackResult;
        Power = power;
        CriticalDamage = criticalDamage;
        AttackType = attackType;
        AttackSource = attackSource;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleProcessorAttackResult" /> для наложение эффекта.
    /// </summary>
    public BattleProcessorAttackResult(AttackResult attackResult, UnitAttackType attackType, UnitAttackSource attackSource, int? power, EffectDuration effectDuration, Unit effectDurationControlUnit,
        IReadOnlyList<UnitAttackSourceProtection>? attackSourceProtections = null,
        IReadOnlyList<UnitAttackTypeProtection>? attackTypeProtections = null,
        UnitType? transformUnitType = null)
    {
        AttackResult = attackResult;
        AttackType = attackType;
        AttackSource = attackSource;
        Power = power;
        EffectDuration = effectDuration;
        EffectDurationControlUnit = effectDurationControlUnit;
        AttackSourceProtections = attackSourceProtections ?? Array.Empty<UnitAttackSourceProtection>();
        AttackTypeProtections = attackTypeProtections ?? Array.Empty<UnitAttackTypeProtection>();
        TransformUnitType = transformUnitType;
    }

    /// <summary>
    /// Результат атаки/действия.
    /// </summary>
    public AttackResult AttackResult { get; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType? AttackType { get; }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource? AttackSource { get; }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int? Power { get; }

    /// <summary>
    /// Критический урон.
    /// </summary>
    public int? CriticalDamage { get; }

    /// <summary>
    /// Длительность эффекта.
    /// </summary>
    public EffectDuration? EffectDuration { get; }

    /// <summary>
    /// Юнит, к ходу которого привязана длительность.
    /// </summary>
    public Unit? EffectDurationControlUnit { get; }

    /// <summary>
    /// Защита от источников атак.
    /// </summary>
    public IReadOnlyList<UnitAttackSourceProtection> AttackSourceProtections { get; } = Array.Empty<UnitAttackSourceProtection>();

    /// <summary>
    /// Защита от типов атак.
    /// </summary>
    public IReadOnlyList<UnitAttackTypeProtection> AttackTypeProtections { get; } = Array.Empty<UnitAttackTypeProtection>();

    /// <summary>
    /// Тип юнита в которого необходимо превратить цель.
    /// </summary>
    public UnitType? TransformUnitType { get; }
}