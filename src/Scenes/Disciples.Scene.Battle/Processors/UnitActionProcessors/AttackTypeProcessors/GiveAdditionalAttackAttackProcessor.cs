using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;
using static Disciples.Scene.Battle.Extensions.UnitAttackProcessorExtensions;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.GiveAdditionalAttack" />.
/// </summary>
internal class GiveAdditionalAttackAttackProcessor : IAttackTypeProcessor
{
    /// <inheritdoc />
    public UnitAttackType AttackType => UnitAttackType.GiveAdditionalAttack;

    /// <inheritdoc />
    public bool CanMainAttackBeSkipped => true;

    /// <inheritdoc />
    public bool CanAttackAfterBattle => false;

    /// <inheritdoc />
    public bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return CanAttackFriend(context) &&
               context.TargetUnit.UnitType.MainAttack.AttackType != UnitAttackType.GiveAdditionalAttack;
    }

    /// <inheritdoc />
    public CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return new CalculatedAttackResult(
            context,
            unitAttack.AttackType,
            unitAttack.AttackSource,
            unitAttack.TotalPower);
    }

    /// <inheritdoc />
    public void ProcessAttack(CalculatedAttackResult attackResult)
    {
        var targetUnit = attackResult.Context.TargetUnit;
        var unitTurnQueue = attackResult.Context.UnitTurnQueue;
        unitTurnQueue.AddUnitAdditionalAttack(targetUnit);
    }
}