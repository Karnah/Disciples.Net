using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик прерывания процесса побега юнита.
/// Используется только во время смерти юнита, когда он пытается сбежать.
/// </summary>
internal class UnitInterruptRetreatingProcessor : IUnitEffectProcessor
{
    /// <summary>
    /// Создать объект типа <see cref="UnitInterruptRetreatingProcessor" />.
    /// </summary>
    public UnitInterruptRetreatingProcessor(Unit targetUnit)
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
        TargetUnit.Effects.IsRetreating = false;
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
    }
}