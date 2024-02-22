using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Контекст для обработчики типа атаки.
/// </summary>
internal class AttackProcessorContext
{
    /// <summary>
    /// Создать объект типа <see cref="AttackProcessorContext" />.
    /// </summary>
    public AttackProcessorContext(Unit currentUnit, Unit targetUnit, Squad currentUnitSquad, Squad targetUnitSquad, UnitTurnQueue unitTurnQueue, int roundNumber)
    {
        CurrentUnit = currentUnit;
        TargetUnit = targetUnit;
        CurrentUnitSquad = currentUnitSquad;
        TargetUnitSquad = targetUnitSquad;
        UnitTurnQueue = unitTurnQueue;
        RoundNumber = roundNumber;
    }

    /// <summary>
    /// Юнит, чей ход сейчас.
    /// Для атаки: юнит, который атакует.
    /// </summary>
    public Unit CurrentUnit { get; }

    /// <summary>
    /// Юнит, который выбран в качестве цели.
    /// </summary>
    public Unit TargetUnit { get; }

    /// <summary>
    /// Отряд текущего юнита.
    /// </summary>
    public Squad CurrentUnitSquad { get; }

    /// <summary>
    /// Отряд юнита-цели.
    /// </summary>
    public Squad TargetUnitSquad { get; }

    /// <summary>
    /// Очередность хода юнитов.
    /// </summary>
    public UnitTurnQueue UnitTurnQueue { get; }

    /// <summary>
    /// Номер раунда битвы.
    /// </summary>
    public int RoundNumber { get; }
}