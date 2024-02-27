using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Пропуск основной атаки, чтобы потом перейти ко второй.
/// </summary>
internal class SkipAttackProcessor : IAttackUnitActionProcessor
{
    /// <summary>
    /// Создать объект типа <see cref="SkipAttackProcessor" />.
    /// </summary>
    public SkipAttackProcessor(Unit targetUnit)
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
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
    }
}