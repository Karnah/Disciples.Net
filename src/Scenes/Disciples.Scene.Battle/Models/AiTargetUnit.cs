using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Юнит цель для атаки ИИ.
/// </summary>
internal class AiTargetUnit
{
    /// <summary>
    /// Создать объект типа <see cref="AiTargetUnit" />.
    /// </summary>
    public AiTargetUnit(Unit unit, UnitActionType? mainAttackResult, UnitActionType? secondaryAttackResult)
    {
        Unit = unit;
        MainAttackResult = mainAttackResult;
        SecondaryAttackResult = secondaryAttackResult;
    }

    /// <summary>
    /// Юнит.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    /// Результат базовой атаки.
    /// </summary>
    public UnitActionType? MainAttackResult { get; }

    /// <summary>
    /// Результат вспомогательной атаки.
    /// </summary>
    public UnitActionType? SecondaryAttackResult { get; }
}