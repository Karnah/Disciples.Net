using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Юнит цель для атаки ИИ.
/// </summary>
internal class AiTargetUnit
{
    /// <summary>
    /// Создать объект типа <see cref="AiTargetUnit" />.
    /// </summary>
    public AiTargetUnit(Unit unit, IAttackUnitActionProcessor? mainAttackProcessor, IAttackUnitActionProcessor? secondaryAttackProcessor)
    {
        Unit = unit;
        MainAttackProcessor = mainAttackProcessor;
        SecondaryAttackProcessor = secondaryAttackProcessor;
    }

    /// <summary>
    /// Юнит.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    /// Обработчик первой атаки.
    /// </summary>
    public IAttackUnitActionProcessor? MainAttackProcessor { get; }

    /// <summary>
    /// Результат базовой атаки.
    /// </summary>
    public UnitActionType? MainAttackResult => MainAttackProcessor?.ActionType;

    /// <summary>
    /// Обработчик второй атаки.
    /// </summary>
    public IAttackUnitActionProcessor? SecondaryAttackProcessor { get; }

    /// <summary>
    /// Результат вспомогательной атаки.
    /// </summary>
    public UnitActionType? SecondaryAttackResult => SecondaryAttackProcessor?.ActionType;
}