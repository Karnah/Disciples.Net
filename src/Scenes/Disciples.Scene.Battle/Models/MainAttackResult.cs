using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Результат выполнения основной атаки.
/// </summary>
internal class MainAttackResult
{
    /// <summary>
    /// Создать объект типа <see cref="MainAttackResult" />.
    /// </summary>
    public MainAttackResult(bool isAlternativeAttackUsed, IReadOnlyList<IAttackUnitActionProcessor> attackProcessors, IReadOnlyList<Unit> secondaryAttackUnits)
    {
        IsAlternativeAttackUsed = isAlternativeAttackUsed;
        AttackProcessors = attackProcessors;
        SecondaryAttackUnits = secondaryAttackUnits;
    }

    /// <summary>
    /// Признак, что использована альтернативная атака.
    /// </summary>
    public bool IsAlternativeAttackUsed { get; }

    /// <summary>
    /// Обработчики атаки.
    /// </summary>
    public IReadOnlyList<IAttackUnitActionProcessor> AttackProcessors { get; }

    /// <summary>
    /// Юниты, на которых будет распространяться вторая атака.
    /// </summary>
    public IReadOnlyList<Unit> SecondaryAttackUnits { get; }
}