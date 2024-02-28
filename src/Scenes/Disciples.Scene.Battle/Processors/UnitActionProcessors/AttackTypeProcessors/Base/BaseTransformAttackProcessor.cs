using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

/// <summary>
/// Базовый класс для атак, которые вызывают превращение.
/// </summary>
internal abstract class BaseTransformAttackProcessor : BaseEffectAttackProcessor
{
    /// <inheritdoc />
    /// <remarks>
    /// Для всех превращений возможно превращение по цепочке несколько раз.
    /// Поэтому эффект всегда можно заменить.
    /// </remarks>
    protected override bool CanReplaceEffect(UnitBattleEffect existingBattleEffect, int? newEffectPower, EffectDuration newEffectDuration)
    {
        return true;
    }

    /// <inheritdoc />
    public override void ProcessAttack(CalculatedAttackResult attackResult)
    {
        var targetUnit = attackResult.Context.TargetUnit;

        // Эффект трансформации может быть только один.
        // Если юнит превратился еще раз, то при обратной трансформации сразу будет возвращён в исходное состояние.
        var transformEffect = targetUnit
            .Effects
            .GetBattleEffects()
            .SingleOrDefault(be =>
                be.AttackType
                    is UnitAttackType.ReduceLevel
                    or UnitAttackType.Doppelganger
                    or UnitAttackType.TransformSelf
                    or UnitAttackType.TransformEnemy);
        if (transformEffect != null)
            targetUnit.Effects.Remove(transformEffect);

        base.ProcessAttack(attackResult);

        // Заменяем в отряде оригинального юнита на трансформированного.
        var targetUnitSquad = attackResult.Context.TargetUnitSquad;
        var transformedUnit = attackResult.TransformedUnit!.Unit;
        var targetUnitSquadIndex = targetUnitSquad.Units.IndexOf(targetUnit);
        targetUnitSquad.Units[targetUnitSquadIndex] = transformedUnit;

        transformedUnit.HitPoints = GetTransformHitPoints(targetUnit, transformedUnit);

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
        var transformedUnit = ((ITransformedUnit)targetUnit).Unit;
        var originalUnit = ((ITransformedUnit)targetUnit).OriginalUnit;
        var targetUnitSquadIndex = targetUnitSquad.Units.IndexOf(targetUnit);
        targetUnitSquad.Units[targetUnitSquadIndex] = originalUnit;

        originalUnit.HitPoints = GetTransformHitPoints(transformedUnit, originalUnit);

        var unitTurnQueue = context.UnitTurnQueue;
        unitTurnQueue.ReorderTransformedUnitTurn(targetUnit, originalUnit, originalUnit.Initiative);
    }

    /// <summary>
    /// Проверить, может ли превратить юнит в указанного.
    /// </summary>
    /// <remarks>
    /// Можно превратить только в юнита того же размера, и нельзя превращать в того же юнита.
    /// </remarks>
    protected static bool CanTransform(Unit targetUnit, UnitType transformUnitType)
    {
        return targetUnit.UnitType.IsSmall == transformUnitType.IsSmall &&
               targetUnit.UnitType.Id != transformUnitType.Id;
    }

    /// <summary>
    /// Получить количество жизней после трансформации юнита.
    /// </summary>
    private static int GetTransformHitPoints(Unit fromUnit, Unit toUnit)
    {
        if (fromUnit.MaxHitPoints == toUnit.MaxHitPoints)
            return fromUnit.HitPoints;

        var hitPointsModifier = (decimal)fromUnit.HitPoints / fromUnit.MaxHitPoints;
        return (int) (toUnit.MaxHitPoints * hitPointsModifier);
    }
}