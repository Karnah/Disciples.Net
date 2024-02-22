using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.DrainLife" />.
/// </summary>
internal class DrainLifeAttackProcessor : BaseDirectDamageAttackProcessor, IDrainLifeAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.DrainLife;

    /// <inheritdoc />
    /// <remarks>
    /// Юнит лечит только себя.
    /// </remarks>
    public IReadOnlyList<Unit> ProcessDrainLifeHeal(IReadOnlyList<CalculatedAttackResult> attackResults)
    {
        var totalHeal = attackResults.Sum(ar => ar.Power!.Value + (ar.CriticalDamage ?? 0)) / 2;
        if (totalHeal == 0)
            return Array.Empty<Unit>();

        var attackingUnit = attackResults[0].Context.CurrentUnit;
        var vampireHealPower = Math.Min(attackingUnit.MaxHitPoints - attackingUnit.HitPoints, totalHeal);
        if (vampireHealPower == 0)
            return Array.Empty<Unit>();

        attackingUnit.HitPoints += vampireHealPower;
        return new[] { attackingUnit };
    }
}