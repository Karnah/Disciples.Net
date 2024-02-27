using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик промаха при атаке.
/// </summary>
internal class MissAttackProcessor : IAttackUnitActionProcessor
{
    /// <summary>
    /// Создать объект типа <see cref="MissAttackProcessor" />.
    /// </summary>
    public MissAttackProcessor(Unit targetUnit)
    {
        TargetUnit = targetUnit;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Miss;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
    }
}