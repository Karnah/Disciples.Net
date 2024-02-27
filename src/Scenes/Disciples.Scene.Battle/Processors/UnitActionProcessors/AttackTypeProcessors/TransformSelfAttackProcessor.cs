using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.TransformSelf" />.
/// </summary>
internal class TransformSelfAttackProcessor : BaseTransformAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.TransformSelf;

    /// <inheritdoc />
    protected override bool CanAttackFriends => true;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return base.CanAttack(context, unitAttack) &&
               context.CurrentUnit.Id == context.TargetUnit.Id;
    }

    /// <inheritdoc />
    protected override ITransformedUnit? GetTransformedUnit(Unit attackingUnit,
        Unit targetUnit, CalculatedUnitAttack unitAttack)
    {
        return new FullTransformUnit(attackingUnit, unitAttack.SummonTransformUnitTypes[0]);
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(CalculatedUnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }

    /// <inheritdoc />
    protected override void ProcessEffectCompleted(AttackProcessorContext context, UnitBattleEffect battleEffect)
    {
        base.ProcessEffectCompleted(context, battleEffect);

        var targetUnit = (ITransformedUnit)context.TargetUnit;
        var transformedUnit = targetUnit.Unit;
        var originalUnit = targetUnit.OriginalUnit;

        var hitPointsModifier = (decimal)transformedUnit.HitPoints / transformedUnit.MaxHitPoints;
        originalUnit.HitPoints = (int) (originalUnit.MaxHitPoints * hitPointsModifier);
    }
}