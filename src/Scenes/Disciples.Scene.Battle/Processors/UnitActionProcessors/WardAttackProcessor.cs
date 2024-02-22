using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Обработчик защиты при атаке.
/// </summary>
internal class WardAttackProcessor : IAttackUnitActionProcessor
{
    private readonly Unit _targetUnit;
    private readonly UnitAttackType _attackType;
    private readonly UnitAttackSource _attackSource;

    /// <summary>
    /// Создать объект типа <see cref="WardAttackProcessor" />.
    /// </summary>
    public WardAttackProcessor(Unit targetUnit, UnitAttackType attackType, UnitAttackSource attackSource)
    {
        _targetUnit = targetUnit;
        _attackType = attackType;
        _attackSource = attackSource;
    }

    public UnitActionType ActionType => UnitActionType.Ward;

    /// <inheritdoc />
    public Unit TargetUnit => _targetUnit;

    /// <inheritdoc />
    public IReadOnlyList<Unit> SecondaryAttackUnits => Array.Empty<Unit>();

    /// <inheritdoc />
    public void ProcessBeginAction()
    {
    }

    /// <inheritdoc />
    public void ProcessCompletedAction()
    {
        var attackTypeProtection = new UnitAttackTypeProtection(_attackType, ProtectionCategory.Ward);
        _targetUnit.BaseAttackTypeProtections.Remove(attackTypeProtection);
        _targetUnit.Effects.RemoveBattleProtectionEffect(attackTypeProtection);

        var attackSourceProtection = new UnitAttackSourceProtection(_attackSource, ProtectionCategory.Ward);
        _targetUnit.BaseAttackSourceProtections.Remove(attackSourceProtection);
        _targetUnit.Effects.RemoveBattleProtectionEffect(attackSourceProtection);
    }
}