using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Doppelganger" />.
/// </summary>
internal class DoppelgangerAttackProcessor : BaseTransformAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.Doppelganger;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    protected override bool CanAttackFriends => true;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return base.CanAttack(context, unitAttack) &&
               CanTransform(context.CurrentUnit, context.TargetUnit.UnitType);
    }

    /// <inheritdoc />
    public override CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context,
        CalculatedUnitAttack unitAttack)
    {
        // Целью превращения является сам атакующий юнит. Цель нужна только для информации во что превращаться.
        var overridenContext = new AttackProcessorContext(context.CurrentUnit, context.CurrentUnit,
            context.CurrentUnitSquad, context.CurrentUnitSquad,
            context.UnitTurnQueue, context.RoundNumber);
        var attackingUnit = context.CurrentUnit;
        var targetUnit = context.TargetUnit;
        return new CalculatedAttackResult(
            overridenContext,
            unitAttack.AttackType,
            unitAttack.AttackSource,
            unitAttack.TotalPower,
            GetEffectDuration(unitAttack, false),
            attackingUnit,
            null,
            null,
            GetTransformedUnit(attackingUnit, targetUnit, unitAttack));
    }

    /// <inheritdoc />
    protected override ITransformedUnit? GetTransformedUnit(Unit attackingUnit,
        Unit targetUnit, CalculatedUnitAttack unitAttack)
    {
        return new FullTransformUnit(attackingUnit, targetUnit.UnitType);
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(CalculatedUnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }
}