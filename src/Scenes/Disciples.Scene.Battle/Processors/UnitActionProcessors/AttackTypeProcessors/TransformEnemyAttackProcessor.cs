using Disciples.Engine;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.TransformEnemy" />.
/// </summary>
internal class TransformEnemyAttackProcessor : BaseEffectAttackProcessor
{
    private const int MIN_TURN_DURATION = 2;
    private const int MAX_TURN_DURATION = 3;

    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.TransformEnemy;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, UnitAttack unitAttack, int? power)
    {
        if (!base.CanAttack(context, unitAttack, power))
            return false;

        // Можно превратить только в юнита того же размера, и нельзя превращать в того же юнита.
        var targetUnitType = context.TargetUnit.UnitType;
        return unitAttack
            .SummonTransformUnitTypes
            .Any(ut => ut.IsSmall == targetUnitType.IsSmall && ut.Id != targetUnitType.Id);
    }

    /// <inheritdoc />
    public override void ProcessAttack(CalculatedAttackResult attackResult)
    {
        base.ProcessAttack(attackResult);

        // Заменяем в отряде оригинального юнита на трансформированного.
        var targetUnit = attackResult.Context.TargetUnit;
        var targetUnitSquad = attackResult.Context.TargetUnitSquad;
        var transformedUnit = attackResult.TransformedUnit!;
        var targetUnitSquadIndex = targetUnitSquad.Units.IndexOf(targetUnit);
        targetUnitSquad.Units[targetUnitSquadIndex] = transformedUnit;

        var unitTurnQueue = attackResult.Context.UnitTurnQueue;
        unitTurnQueue.ReorderTransformedUnitTurn(targetUnit, transformedUnit, transformedUnit.Initiative);
    }

    /// <inheritdoc />
    /// <remarks>
    /// После завершения эффекта возвращаем оригинального юнита в отряд.
    /// </remarks>
    protected override void ProcessEffectCompleted(AttackProcessorContext context, UnitBattleEffect battleEffect)
    {
        base.ProcessEffectCompleted(context, battleEffect);

        var targetUnit = context.TargetUnit;
        var targetUnitSquad = context.TargetUnitSquad;
        var originalUnit = ((TransformedEnemyUnit)targetUnit).OriginalUnit;
        var targetUnitSquadIndex = targetUnitSquad.Units.IndexOf(targetUnit);
        targetUnitSquad.Units[targetUnitSquadIndex] = originalUnit;

        var unitTurnQueue = context.UnitTurnQueue;
        unitTurnQueue.ReorderTransformedUnitTurn(targetUnit, originalUnit, originalUnit.Initiative);
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(UnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? isMaximum
                ? EffectDuration.Create(MAX_TURN_DURATION)
                : EffectDuration.CreateRandom(MIN_TURN_DURATION, MAX_TURN_DURATION)
            : EffectDuration.Create(MIN_TURN_DURATION);
    }

    /// <inheritdoc />
    protected override TransformedEnemyUnit GetTransformedUnit(Unit attackingUnit, UnitAttack unitAttack,
        Unit targetUnit)
    {
        var transformUnits = unitAttack
            .SummonTransformUnitTypes
            .Where(ut => ut.IsSmall == targetUnit.UnitType.IsSmall && ut.Id != targetUnit.UnitType.Id)
            .ToArray();
        var transformedUnitType = transformUnits[RandomGenerator.Get(transformUnits.Length)];
        return new TransformedEnemyUnit(targetUnit, transformedUnitType);
    }
}