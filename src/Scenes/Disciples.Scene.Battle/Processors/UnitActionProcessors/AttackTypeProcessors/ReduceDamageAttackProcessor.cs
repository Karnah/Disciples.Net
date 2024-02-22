using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.ReduceDamage" />.
/// </summary>
internal class ReduceDamageAttackProcessor : BaseEffectAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.ReduceDamage;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(UnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }
}