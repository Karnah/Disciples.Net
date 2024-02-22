using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик иммунитета при атаке.
/// </summary>
internal class ImmunityAttackProcessor : IAttackUnitActionProcessor
{
    /// <summary>
    /// Создать объект типа <see cref="ImmunityAttackProcessor" />.
    /// </summary>
    public ImmunityAttackProcessor(Unit targetUnit)
    {
        TargetUnit = targetUnit;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Immunity;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <inheritdoc />
    public IReadOnlyList<Unit> SecondaryAttackUnits => Array.Empty<Unit>();

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
    }
}