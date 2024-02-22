using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;
using static Disciples.Scene.Battle.Extensions.UnitAttackProcessorExtensions;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Heal" />.
/// </summary>
internal class HealAttackProcessor : IAttackTypeProcessor
{
    /// <inheritdoc />
    public UnitAttackType AttackType => UnitAttackType.Heal;

    /// <inheritdoc />
    public bool CanMainAttackBeSkipped => true;

    /// <inheritdoc />
    public bool CanAttack(AttackProcessorContext context, UnitAttack unitAttack, int? power)
    {
        return CanAttackFriend(context) &&
               context.TargetUnit.HitPoints < context.TargetUnit.MaxHitPoints;
    }

    /// <inheritdoc />
    public CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, UnitAttack unitAttack,
        int? power, int? basePower)
    {
        if (power == null)
            throw new ArgumentNullException(nameof(power));

        var targetUnit = context.TargetUnit;
        var healPower = Math.Min(power.Value, targetUnit.MaxHitPoints - targetUnit.HitPoints);
        return new CalculatedAttackResult(
            context,
            unitAttack.AttackType,
            unitAttack.AttackSource,
            healPower);
    }

    /// <inheritdoc />
    public void ProcessAttack(CalculatedAttackResult attackResult)
    {
        var targetUnit = attackResult.Context.TargetUnit;
        targetUnit.HitPoints += attackResult.Power!.Value;
    }
}