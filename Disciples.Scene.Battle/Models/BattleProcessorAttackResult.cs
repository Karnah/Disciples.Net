using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Результат атаки одного юнита на другого.
/// </summary>
internal class BattleProcessorAttackResult
{
    public BattleProcessorAttackResult(AttackResult attackResult)
    {
        AttackResult = attackResult;
    }

    public BattleProcessorAttackResult(AttackResult attackResult, int power, UnitAttackType attackType)
    {
        AttackResult = attackResult;
        Power = power;
        AttackType = attackType;
    }

    public BattleProcessorAttackResult(AttackResult attackResult, int? power, int roundDuration, UnitAttackType attackType)
    {
        AttackResult = attackResult;
        Power = power;
        RoundDuration = roundDuration;
        AttackType = attackType;
    }

    public AttackResult AttackResult { get; }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int? Power { get; }

    /// <summary>
    /// Длительность эффекта в раундах.
    /// </summary>
    public int? RoundDuration { get; set; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType? AttackType { get; }
}