using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик смерти юнита.
/// </summary>
internal class UnitDeathProcessor : IUnitActionProcessor
{
    private readonly Squad _enemySquad;
    private readonly IReadOnlyList<IUnitEffectProcessor> _unitEffectProcessors;

    /// <summary>
    /// Создать объект типа <see cref="UnitDeathProcessor" />.
    /// </summary>
    public UnitDeathProcessor(Unit targetUnit, Squad enemySquad, IReadOnlyList<IUnitEffectProcessor> unitEffectProcessors, IReadOnlyList<IUnitEffectProcessor> unsummonProcessors)
    {
        TargetUnit = targetUnit;
        _enemySquad = enemySquad;
        _unitEffectProcessors = unitEffectProcessors;
        UnsummonProcessors = unsummonProcessors;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Dying;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <summary>
    /// Обработчики для удаления вызванных юнитов.
    /// </summary>
    public IReadOnlyList<IUnitEffectProcessor> UnsummonProcessors { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
        TargetUnit.IsDead = true;

        // Начисляем опыт вражескому отряду.
        var experienceEnemies = _enemySquad
            .Units
            .Where(x => x is { IsInactive: false, Effects.IsSummoned: false })
            .ToArray();
        if (experienceEnemies.Length > 0)
        {
            var experience = TargetUnit.DeathExperience / experienceEnemies.Length;
            foreach (var enemy in experienceEnemies)
                enemy.BattleExperience += experience;
        }

        foreach (var unitEffectProcessor in _unitEffectProcessors)
        {
            unitEffectProcessor.ProcessBeginAction();
        }

        foreach (var unsummonProcessor in UnsummonProcessors)
        {
            unsummonProcessor.ProcessBeginAction();
        }
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
        foreach (var unitEffectProcessor in _unitEffectProcessors)
        {
            unitEffectProcessor.ProcessCompletedAction();
        }

        foreach (var unsummonProcessor in UnsummonProcessors)
        {
            unsummonProcessor.ProcessCompletedAction();
        }
    }
}