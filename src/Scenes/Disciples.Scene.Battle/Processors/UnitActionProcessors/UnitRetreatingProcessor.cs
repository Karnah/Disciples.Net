using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Отступление юнита с поля боя.
/// </summary>
internal class UnitRetreatingProcessor : IUnitActionProcessor
{
    /// <summary>
    /// Создать объект типа <see cref="UnitRetreatingProcessor" />.
    /// </summary>
    public UnitRetreatingProcessor(Unit targetUnit)
    {
        TargetUnit = targetUnit;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Retreating;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
        TargetUnit.Effects.IsRetreating = true;
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
    }
}