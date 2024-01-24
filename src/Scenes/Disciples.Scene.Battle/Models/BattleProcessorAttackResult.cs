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
    public BattleProcessorAttackResult(AttackResult attackResult, UnitAttackType attackType,
        UnitAttackSource attackSource)
    {
        AttackResult = attackResult;
        AttackType = attackType;
        AttackSource = attackSource;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleProcessorAttackResult" />.
    /// </summary>
    public BattleProcessorAttackResult(AttackResult attackResult, int power, int? criticalDamage, UnitAttackType attackType, UnitAttackSource attackSource)
    {
        AttackResult = attackResult;
        Power = power;
        CriticalDamage = criticalDamage;
        AttackType = attackType;
        AttackSource = attackSource;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleProcessorAttackResult" />.
    /// </summary>
    public BattleProcessorAttackResult(AttackResult attackResult, int? power, EffectDuration effectDuration, Unit effectDurationControlUnit, UnitAttackType attackType, UnitAttackSource attackSource)
    {
        AttackResult = attackResult;
        Power = power;
        EffectDuration = effectDuration;
        EffectDurationControlUnit = effectDurationControlUnit;
        AttackType = attackType;
        AttackSource = attackSource;
    }

    /// <summary>
    /// Результат атаки/действия.
    /// </summary>
    public AttackResult AttackResult { get; }

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
    /// Тип атаки.
    /// </summary>
    public UnitAttackType? AttackType { get; }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource? AttackSource { get; }
}