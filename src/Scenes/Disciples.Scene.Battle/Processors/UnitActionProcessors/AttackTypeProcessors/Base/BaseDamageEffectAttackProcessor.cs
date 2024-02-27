using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

/// <summary>
/// Базовый класс для эффектов, которые наносят периодический урон.
/// </summary>
internal abstract class BaseDamageEffectAttackProcessor : BaseEffectAttackProcessor
{
    private const int MIN_TURN_DURATION = 2;
    private const int MAX_TURN_DURATION = 4;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    public override void ProcessEffect(CalculatedEffectResult effectResult)
    {
        base.ProcessEffect(effectResult);

        if (effectResult.Power > 0)
        {
            var targetUnit = effectResult.Context.TargetUnit;
            targetUnit.HitPoints -= effectResult.Power.Value;
        }
    }

    /// <inheritdoc />
    protected override int? GetEffectPower(Unit targetUnit, UnitBattleEffect effect)
    {
        return Math.Min(targetUnit.HitPoints, effect.Power!.Value);
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