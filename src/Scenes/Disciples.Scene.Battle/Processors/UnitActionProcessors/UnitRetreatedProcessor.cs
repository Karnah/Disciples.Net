using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик окончательного побега юнита с поля боя.
/// </summary>
internal class UnitRetreatedProcessor : IUnitEffectProcessor
{
    private readonly IReadOnlyList<IUnitEffectProcessor> _unitEffectProcessors;

    /// <summary>
    /// Создать объект тип <see cref="UnitRetreatedProcessor" />.
    /// </summary>
    public UnitRetreatedProcessor(Unit targetUnit, IReadOnlyList<IUnitEffectProcessor> unitEffectProcessors)
    {
        TargetUnit = targetUnit;
        _unitEffectProcessors = unitEffectProcessors;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Attacked;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
        TargetUnit.IsRetreated = true;

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