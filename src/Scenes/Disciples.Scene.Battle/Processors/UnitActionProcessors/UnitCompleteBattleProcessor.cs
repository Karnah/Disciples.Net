using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Получение опыта юнитом.
/// </summary>
internal class UnitCompleteBattleProcessor : IUnitActionProcessor
{
    private readonly Squad _targetUnitSquad;
    private readonly IReadOnlyList<IUnitEffectProcessor> _unitEffectProcessors;

    /// <summary>
    /// Создать объект типа <see cref="UnitCompleteBattleProcessor" />.
    /// </summary>
    public UnitCompleteBattleProcessor(Unit targetUnit, Unit? levelUpUnit, Squad targetUnitSquad, IReadOnlyList<IUnitEffectProcessor> unitEffectProcessors)
    {
        TargetUnit = targetUnit;
        LevelUpUnit = levelUpUnit;
        _targetUnitSquad = targetUnitSquad;
        _unitEffectProcessors = unitEffectProcessors;
    }

    /// <inheritdoc />
    public UnitActionType ActionType => UnitActionType.Experience;

    /// <inheritdoc />
    public Unit TargetUnit { get; }

    /// <summary>
    /// Если юнит получил новый уровень, то данные нового юнита.
    /// </summary>
    public Unit? LevelUpUnit { get; }

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
        // Снимаем все оставшиеся эффекты.
        foreach (var unitEffectProcessor in _unitEffectProcessors)
        {
            unitEffectProcessor.ProcessBeginAction();
            unitEffectProcessor.ProcessCompletedAction();
        }
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
        if (LevelUpUnit != null)
        {
            // Заменяем юнита в отряде на нового.
            var targetUnitSquadIndex = _targetUnitSquad.Units.IndexOf(TargetUnit);
            _targetUnitSquad.Units[targetUnitSquadIndex] = LevelUpUnit;

            return;
        }

        TargetUnit.Experience += TargetUnit.BattleExperience;
    }
}