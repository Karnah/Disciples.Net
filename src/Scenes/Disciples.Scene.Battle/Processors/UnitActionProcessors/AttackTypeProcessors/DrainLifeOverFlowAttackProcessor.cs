using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.DrainLifeOverflow" />.
/// </summary>
internal class DrainLifeOverFlowAttackProcessor : BaseDirectDamageAttackProcessor, IDrainLifeAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.DrainLifeOverflow;

    /// <inheritdoc />
    /// <remarks>
    /// Юнит в первую очередь лечит себя.
    /// Если он полностью здоров/вылечился, то остаток лечения равномерно распределяется между ранеными союзниками.
    /// </remarks>
    public IReadOnlyList<Unit> ProcessDrainLifeHeal(IReadOnlyList<CalculatedAttackResult> attackResults)
    {
        var totalHeal = attackResults.Sum(ar => ar.Power!.Value + (ar.CriticalDamage ?? 0)) / 2;
        if (totalHeal == 0)
            return Array.Empty<Unit>();

        var healedUnits = new List<Unit>();

        var attackingUnit = attackResults[0].Context.CurrentUnit;
        var vampireHealPower = Math.Min(attackingUnit.MaxHitPoints - attackingUnit.HitPoints, totalHeal);
        if (vampireHealPower > 0)
        {
            attackingUnit.HitPoints += vampireHealPower;
            totalHeal -= vampireHealPower;
            healedUnits.Add(attackingUnit);
        }

        if (totalHeal == 0)
            return healedUnits;

        // Лечение остальных юнитов делится поровну между всеми.
        // Но если есть слабо раненые юниты (нужно восстановить меньше, чем среднее), то остальные получат больше.
        // Для того чтобы правильно это обработать, сортируем от менее раненных к более раненым.
        var damagedUnits = attackResults[0]
            .Context
            .CurrentUnitSquad
            .Units
            .Where(u => u != attackingUnit && !u.IsDeadOrRetreated && u.HitPoints < u.MaxHitPoints)
            .OrderBy(u => u.MaxHitPoints - u.HitPoints)
            .ToArray();
        for (var unitIndex = 0; unitIndex < damagedUnits.Length; unitIndex++)
        {
            var targetUnit = damagedUnits[unitIndex];
            var unitHealPower = Math.Min(
                targetUnit.MaxHitPoints - targetUnit.HitPoints,
                totalHeal / (damagedUnits.Length - unitIndex));
            targetUnit.HitPoints += unitHealPower;
            totalHeal -= unitHealPower;
            healedUnits.Add(targetUnit);
        }

        return healedUnits;
    }
}