using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Контекст для обработчики типа атаки.
/// </summary>
internal class AttackProcessorContext : BattleProcessorContext
{
    /// <summary>
    /// Создать объект типа <see cref="AttackProcessorContext" />.
    /// </summary>
    public AttackProcessorContext(Unit currentUnit, Unit targetUnit,
        Squad attackingSquad, Squad defendingSquad,
        UnitTurnQueue unitTurnQueue, int roundNumber
        ) : base(currentUnit, attackingSquad, defendingSquad, unitTurnQueue, roundNumber)
    {
        TargetUnit = targetUnit;
        TargetUnitSquad = GetUnitSquad(targetUnit);
    }

    /// <summary>
    /// Юнит, который выбран в качестве цели.
    /// </summary>
    public Unit TargetUnit { get; }

    /// <summary>
    /// Отряд юнита-цели.
    /// </summary>
    public Squad TargetUnitSquad { get; }
}