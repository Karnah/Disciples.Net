using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик ожидания юнита.
/// </summary>
internal class UnitWaitingProcessor : IUnitActionProcessor
{
    private readonly UnitTurnQueue _turnQueue;

    /// <summary>
    /// Создать объект типа <see cref="UnitWaitingProcessor" />.
    /// </summary>
    public UnitWaitingProcessor(Unit targetUnit, UnitTurnQueue turnQueue)
    {
        TargetUnit = targetUnit;
        _turnQueue = turnQueue;
    }

    public UnitActionType ActionType => UnitActionType.Waiting;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
        _turnQueue.UnitWait(TargetUnit);
    }
}