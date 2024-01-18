using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Extensions;

/// <summary>
/// Расширения для логики ИИ битвы.
/// </summary>
internal static class BattleAiExtensions
{
    /// <summary>
    /// Отсортировать юнитов по возрастанию оставшегося здоровья.
    /// </summary>
    public static IOrderedEnumerable<AiTargetUnit> OrderByWeakness(this IEnumerable<AiTargetUnit> units)
    {
        return units.OrderBy(u => u.Unit.HitPoints);
    }

    /// <summary>
    /// Отсортировать юнитов по возрастанию оставшегося здоровья.
    /// </summary>
    public static IOrderedEnumerable<AiTargetUnit> ThenByWeakness(this IOrderedEnumerable<AiTargetUnit> units)
    {
        return units.ThenBy(u => u.Unit.HitPoints);
    }

    /// <summary>
    /// Отсортировать юнитов по убыванию урона.
    /// </summary>
    public static IOrderedEnumerable<AiTargetUnit> OrderByPower(this IEnumerable<AiTargetUnit> units)
    {
        return units.OrderByDescending(PowerPriority);
    }

    /// <summary>
    /// Отсортировать юнитов по убыванию урона.
    /// </summary>
    public static IOrderedEnumerable<AiTargetUnit> ThenByPower(this IOrderedEnumerable<AiTargetUnit> units)
    {
        return units.ThenByDescending(PowerPriority);
    }

    /// <summary>
    /// Приоритет по силе.
    /// </summary>
    private static int PowerPriority(AiTargetUnit targetUnit)
    {
        // BUG Здесь некорректно будет отрабатывать в случае эффектов усиления/ослабления.
        var power = targetUnit.Unit.MainAttackPower + (targetUnit.Unit.SecondaryAttackPower ?? 0);

        if (targetUnit.Unit.UnitType.IsAttackTwice)
            power *= 2;

        // BUG Вообще правильно считать по количеству целей.
        // Но для простоты умножаем в три раза.
        if (targetUnit.Unit.UnitType.MainAttack.Reach == UnitAttackReach.All)
            power *= 3;

        return power;
    }
}