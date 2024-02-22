using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Revive" />.
/// </summary>
internal class ReviveAttackProcessor : IAttackTypeProcessor
{
    /// <inheritdoc />
    public UnitAttackType AttackType => UnitAttackType.Revive;

    /// <inheritdoc />
    public bool CanMainAttackBeSkipped => true;

    /// <inheritdoc />
    public bool CanAttack(AttackProcessorContext context, UnitAttack unitAttack, int? power)
    {
        var attackingUnit = context.CurrentUnit;
        var targetUnit = context.TargetUnit;

        // Не используем CanAttackFriend, так как там возвращается false, если юнит мёртв.
        return attackingUnit.Player.Id == targetUnit.Player.Id &&
               targetUnit.IsDead &&
               !targetUnit.IsRevived;
    }

    /// <inheritdoc />
    public CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, UnitAttack unitAttack,
        int? power, int? basePower)
    {
        return new CalculatedAttackResult(
            context,
            unitAttack.AttackType,
            unitAttack.AttackSource,
            power);
    }

    /// <inheritdoc />
    public void ProcessAttack(CalculatedAttackResult attackResult)
    {
        var targetUnit = attackResult.Context.TargetUnit;
        targetUnit.IsDead = false;
        targetUnit.IsRevived = true;
        targetUnit.HitPoints = targetUnit.MaxHitPoints / 2;
    }
}