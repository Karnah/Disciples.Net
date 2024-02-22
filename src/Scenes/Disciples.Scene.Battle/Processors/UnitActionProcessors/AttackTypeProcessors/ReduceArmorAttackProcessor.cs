using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.ReduceArmor" />.
/// </summary>
/// <remarks>
/// TODO В теории, можно обработчик переписать как множественный эффект,
/// т.е. с IsSingleEffectOnly = false.
/// </remarks>
internal class ReduceArmorAttackProcessor : BaseEffectAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.ReduceArmor;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, UnitAttack unitAttack, int? power)
    {
        power = GetTotalPower(power, context.TargetUnit);
        return base.CanAttack(context, unitAttack, power);
    }

    /// <inheritdoc />
    public override CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context,
        UnitAttack unitAttack, int? power, int? basePower)
    {
        power = GetTotalPower(power, context.TargetUnit);
        return base.CalculateAttackResult(context, unitAttack, power, basePower);
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(UnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }

    /// <summary>
    /// Получить итоговую силу разрушения брони для юнита.
    /// </summary>
    private static int GetTotalPower(int? power, Unit targetUnit)
    {
        // Эффект разрушения брони складывается.
        targetUnit.Effects.TryGetBattleEffect(UnitAttackType.ReduceArmor, out var reduceArmorBattleEffect);
        return Math.Min(power!.Value, targetUnit.Armor) + (reduceArmorBattleEffect?.Power ?? 0);
    }
}