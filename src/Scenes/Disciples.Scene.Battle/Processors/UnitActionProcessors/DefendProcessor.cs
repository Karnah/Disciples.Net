using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик защиты юнита.
/// </summary>
internal class DefendProcessor : IUnitActionProcessor
{
    /// <summary>
    /// Создать объект типа <see cref="DefendProcessor" />.
    /// </summary>
    public DefendProcessor(Unit targetUnit)
    {
        TargetUnit = targetUnit;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Defend;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
        TargetUnit.Effects.IsDefended = true;
    }
}