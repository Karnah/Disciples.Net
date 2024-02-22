using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик завершения действия защиты.
/// </summary>
internal class DefendCompletedProcessor : IUnitEffectProcessor
{
    /// <summary>
    /// Создать объект типа <see cref="DefendCompletedProcessor" />.
    /// </summary>
    public DefendCompletedProcessor(Unit targetUnit)
    {
        TargetUnit = targetUnit;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Attacked;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
        TargetUnit.Effects.IsDefended = false;
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
    }
}