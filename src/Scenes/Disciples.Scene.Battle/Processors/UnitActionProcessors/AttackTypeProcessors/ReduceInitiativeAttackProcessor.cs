using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.ReduceInitiative" />.
/// </summary>
internal class ReduceInitiativeAttackProcessor : BaseEffectAttackProcessor
{
    private const int MIN_TURN_DURATION = 2;
    private const int MAX_TURN_DURATION = 4;

    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.ReduceInitiative;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    public override void ProcessAttack(CalculatedAttackResult attackResult)
    {
        base.ProcessAttack(attackResult);

        // Если уменьшилась инициатива, то в очередь его засовываем без учёта случайного разброса.
        // В каких-то особых случаях, это уменьшит вероятность того, что у него инициатива станет в ходу больше, чем была.
        var targetUnit = attackResult.Context.TargetUnit;
        var unitTurnQueue = attackResult.Context.UnitTurnQueue;
        unitTurnQueue.ReorderUnitTurnOrder(targetUnit, targetUnit.Initiative);
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(UnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : isMaximum
                ? EffectDuration.Create(MAX_TURN_DURATION)
                : EffectDuration.CreateRandom(MIN_TURN_DURATION, MAX_TURN_DURATION);
    }
}