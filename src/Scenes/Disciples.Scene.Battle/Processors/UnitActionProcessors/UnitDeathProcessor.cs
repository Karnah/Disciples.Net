using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик смерти юнита.
/// </summary>
internal class UnitDeathProcessor : IUnitActionProcessor
{
    private readonly IReadOnlyList<IUnitEffectProcessor> _unitEffectProcessors;

    /// <summary>
    /// Создать объект типа <see cref="UnitDeathProcessor" />.
    /// </summary>
    public UnitDeathProcessor(Unit targetUnit, IReadOnlyList<IUnitEffectProcessor> unitEffectProcessors)
    {
        TargetUnit = targetUnit;
        _unitEffectProcessors = unitEffectProcessors;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Dying;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
        TargetUnit.IsDead = true;

        foreach (var unitEffectProcessor in _unitEffectProcessors)
        {
            unitEffectProcessor.ProcessBeginAction();
        }
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
        foreach (var unitEffectProcessor in _unitEffectProcessors)
        {
            unitEffectProcessor.ProcessCompletedAction();
        }
    }
}