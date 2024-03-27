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
    public UnitDeathProcessor(Unit targetUnit, Squad enemySquad, IReadOnlyList<IUnitEffectProcessor> unitEffectProcessors)
    {
        TargetUnit = targetUnit;
        _enemySquad = enemySquad;
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

        // Начисляем опыт вражеском отряду.
        var experienceEnemies = _enemySquad
            .Units
            .Where(x => !x.IsDeadOrRetreated)
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