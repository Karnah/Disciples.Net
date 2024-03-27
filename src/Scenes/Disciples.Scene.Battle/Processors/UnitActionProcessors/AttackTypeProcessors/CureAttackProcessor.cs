using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;
using static Disciples.Scene.Battle.Extensions.UnitAttackProcessorExtensions;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Cure" />.
/// </summary>
internal class CureAttackProcessor : IAttackTypeProcessor
{
    private readonly Lazy<IReadOnlyDictionary<UnitAttackType, IEffectAttackProcessor>> _effectAttackProcessors;

    /// <summary>
    /// Создать объект типа <see cref="CureAttackProcessor" />.
    /// </summary>
    public CureAttackProcessor(Lazy<IReadOnlyDictionary<UnitAttackType, IAttackTypeProcessor>> attackProcessors)
    {
        _effectAttackProcessors = new Lazy<IReadOnlyDictionary<UnitAttackType, IEffectAttackProcessor>>(() => attackProcessors
            .Value
            .Values
            .OfType<IEffectAttackProcessor>()
            .ToDictionary(eap => eap.AttackType, eap => eap));
    }

    /// <inheritdoc />
    public UnitAttackType AttackType => UnitAttackType.Cure;

    /// <inheritdoc />
    public bool CanMainAttackBeSkipped => true;

    /// <inheritdoc />
    public bool CanAttackAfterBattle => false;

    /// <inheritdoc />
    public bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return CanAttackFriend(context) &&
               GetCurableEffects(context.TargetUnit).Any();
    }

    /// <inheritdoc />
    public CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        var curableEffects = GetCurableEffects(context.TargetUnit).ToArray();
        return new CalculatedAttackResult(
            context,
            unitAttack.AttackType,
            unitAttack.AttackSource,
            unitAttack.TotalPower,
            curableEffects);
    }

    /// <inheritdoc />
    public void ProcessAttack(CalculatedAttackResult attackResult)
    {
        foreach (var battleEffect in attackResult.CuredEffects)
        {
            var effectProcessor = _effectAttackProcessors.Value[battleEffect.AttackType];
            var calculateEffect = effectProcessor.CalculateEffect(attackResult.Context, battleEffect, true);
            if (calculateEffect == null)
            {
                // TODO Logger.Fatal
                continue;
            }

            effectProcessor.ProcessEffect(calculateEffect);
        }
    }

    /// <summary>
    /// Получить эффекты, которые можно исцелить.
    /// </summary>
    private IEnumerable<UnitBattleEffect> GetCurableEffects(Unit targetUnit)
    {
        return targetUnit
            .Effects
            .GetBattleEffects()
            .Where(be => _effectAttackProcessors.Value[be.AttackType].CanCure(be));
    }
}