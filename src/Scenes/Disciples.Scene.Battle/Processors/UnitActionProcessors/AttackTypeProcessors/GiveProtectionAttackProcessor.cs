using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.GiveProtection" />.
/// </summary>
internal class GiveProtectionAttackProcessor : BaseEffectAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.GiveProtection;

    /// <inheritdoc />
    protected override bool CanAttackFriends => true;

    /// <inheritdoc />
    protected override bool IsSingleEffectOnly => false;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        if (!base.CanAttack(context, unitAttack))
            return false;

        var attackTypeProtections = GetAttackTypeProtections(unitAttack, context.TargetUnit);
        var attackSourceProtections = GetAttackSourceProtections(unitAttack, context.TargetUnit);
        return attackTypeProtections.Count > 0 || attackSourceProtections.Count > 0;
    }

    /// <inheritdoc />
    protected override IReadOnlyList<UnitAttackTypeProtection> GetAttackTypeProtections(CalculatedUnitAttack unitAttack,
        Unit targetUnit)
    {
        return unitAttack
            .AttackTypeProtections
            .Where(atp => !targetUnit
                .AttackTypeProtections
                .Any(tatp => tatp == atp))
            .ToArray();
    }

    /// <inheritdoc />
    protected override IReadOnlyList<UnitAttackSourceProtection> GetAttackSourceProtections(
        CalculatedUnitAttack unitAttack, Unit targetUnit)
    {
        return unitAttack
            .AttackSourceProtections
            .Where(asp => !targetUnit
                .AttackSourceProtections
                .Any(tasp => tasp == asp))
            .ToArray();
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(CalculatedUnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }
}