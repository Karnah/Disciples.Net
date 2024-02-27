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
                    is UnitAttackType.Doppelganger
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
        var originalUnit = ((ITransformedUnit)targetUnit).OriginalUnit;
        var targetUnitSquadIndex = targetUnitSquad.Units.IndexOf(targetUnit);
        targetUnitSquad.Units[targetUnitSquadIndex] = originalUnit;

        var unitTurnQueue = context.UnitTurnQueue;
        unitTurnQueue.ReorderTransformedUnitTurn(targetUnit, originalUnit, originalUnit.Initiative);

        // Пересчитываем здоровье юнита после превращения.
        var transformedUnit = ((ITransformedUnit)targetUnit).Unit;
        if (originalUnit.MaxHitPoints == transformedUnit.MaxHitPoints)
        {
            originalUnit.HitPoints = transformedUnit.HitPoints;
        }
        else
        {
            var hitPointsModifier = (decimal)transformedUnit.HitPoints / transformedUnit.MaxHitPoints;
            originalUnit.HitPoints = (int) (originalUnit.MaxHitPoints * hitPointsModifier);
        }
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
}