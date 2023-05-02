using Disciples.Engine.Common.Enums.Units;
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
    /// Создать объект типа <see cref="BattleProcessorAttackResult" />.
    /// </summary>
    public BattleProcessorAttackResult(AttackResult attackResult, int power, UnitAttackType attackType, UnitAttackSource attackSource)
    {
        AttackResult = attackResult;
        Power = power;
        AttackType = attackType;
        AttackSource = attackSource;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleProcessorAttackResult" />.
    /// </summary>
    public BattleProcessorAttackResult(AttackResult attackResult, int? power, int roundDuration, UnitAttackType attackType, UnitAttackSource attackSource)
    {
        AttackResult = attackResult;
        Power = power;
        RoundDuration = roundDuration;
        AttackType = attackType;
        AttackSource = attackSource;
    }

    public AttackResult AttackResult { get; }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int? Power { get; }

    /// <summary>
    /// Длительность эффекта в раундах.
    /// </summary>
    public int? RoundDuration { get; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType? AttackType { get; }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource? AttackSource { get; }
}