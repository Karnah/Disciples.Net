using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.ReduceArmor" />.
/// </summary>
internal class ReduceArmorAttackProcessor : BaseEffectAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.ReduceArmor;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    protected override bool IsSingleEffectOnly => false;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return base.CanAttack(context, unitAttack) &&
               context.TargetUnit.Armor > 0;
    }

    /// <inheritdoc />
    protected override int GetPower(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return Math.Min(context.TargetUnit.Armor, unitAttack.TotalPower);
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(CalculatedUnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }
}