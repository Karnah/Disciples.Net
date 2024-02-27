using Disciples.Engine;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.TransformEnemy" />.
/// </summary>
internal class TransformEnemyAttackProcessor : BaseTransformAttackProcessor
{
    private const int MIN_TURN_DURATION = 2;
    private const int MAX_TURN_DURATION = 3;

    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.TransformEnemy;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        if (!base.CanAttack(context, unitAttack))
            return false;

        return unitAttack
            .SummonTransformUnitTypes
            .Any(ut => CanTransform(context.TargetUnit, ut));
    }

    /// <inheritdoc />
    protected override ITransformedUnit? GetTransformedUnit(Unit attackingUnit,
        Unit targetUnit, CalculatedUnitAttack unitAttack)
    {
        var transformUnits = unitAttack
            .SummonTransformUnitTypes
            .Where(ut => ut.IsSmall == targetUnit.UnitType.IsSmall && ut.Id != targetUnit.UnitType.Id)
            .ToArray();
        var transformedUnitType = transformUnits[RandomGenerator.Get(transformUnits.Length)];
        return new TransformedEnemyUnit(targetUnit, transformedUnitType);
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(CalculatedUnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? isMaximum
                ? EffectDuration.Create(MAX_TURN_DURATION)
                : EffectDuration.CreateRandom(MIN_TURN_DURATION, MAX_TURN_DURATION)
            : EffectDuration.Create(MIN_TURN_DURATION);
    }
}