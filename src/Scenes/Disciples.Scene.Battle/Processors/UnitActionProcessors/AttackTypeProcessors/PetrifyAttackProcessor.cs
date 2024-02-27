using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Petrify" />.
/// </summary>
internal class PetrifyAttackProcessor : BaseEffectAttackProcessor
{
    private const int MIN_TURN_DURATION = 1;
    private const int MAX_TURN_DURATION = 3;

    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.Petrify;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

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