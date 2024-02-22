using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Extensions;

/// <summary>
/// Расширения для обработчиков атаки.
/// </summary>
internal static class UnitAttackProcessorExtensions
{
    #region Проверка атаки врага / союзника.

    /// <summary>
    /// Проверить, можно ли нанести вред врагу.
    /// </summary>
    public static bool CanAttackEnemy(AttackProcessorContext context)
    {
        if (context.CurrentUnit.Player.Id == context.TargetUnit.Player.Id)
            return false;

        if (context.TargetUnit.IsDeadOrRetreated)
            return false;

        if (!CanAttackPosition(context.CurrentUnit, context.TargetUnit, context.CurrentUnitSquad, context.TargetUnitSquad))
            return false;

        return true;
    }

    /// <summary>
    /// Проверить, можно ли наложить эффект на союзника.
    /// </summary>
    public static bool CanAttackFriend(AttackProcessorContext context)
    {
        if (context.CurrentUnit.Player.Id != context.TargetUnit.Player.Id)
            return false;

        if (context.TargetUnit.IsDeadOrRetreated)
            return false;

        return true;
    }

    #endregion

    #region Проверка позиции для атаки

    /// <summary>
    /// Проверить можно ли провести атаку с учётом позиций юнита.
    /// </summary>
    private static bool CanAttackPosition(Unit attackingUnit, Unit targetUnit, Squad attackingSquad, Squad targetSquad)
    {
        if (attackingUnit.UnitType.MainAttack.Reach is UnitAttackReach.All or UnitAttackReach.Any)
            return true;

        // Атакующий юнит сзади, впереди мешает линия союзников.
        if (attackingUnit.SquadLinePosition == UnitSquadLinePosition.Back && !IsFrontLineEmpty(attackingSquad))
            return false;

        // Цель находится на задней линии, но на передней линии есть юниты.
        if (targetUnit.SquadLinePosition == UnitSquadLinePosition.Back && !IsFrontLineEmpty(targetSquad))
            return false;

        // Проверка, может ли юнит дотянуться до врага.
        if (!CanAttackOnFlank(
                attackingUnit.SquadFlankPosition,
                targetUnit.SquadFlankPosition,
                targetUnit.SquadLinePosition,
                targetSquad))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Проверить, свободна ли первая линия в отряде.
    /// </summary>
    private static bool IsFrontLineEmpty(Squad squad)
    {
        return !squad.Units.Any(u => u.SquadLinePosition == UnitSquadLinePosition.Front && !u.IsDeadOrRetreated);
    }

    /// <summary>
    /// Проверить, можно ли атаковать цель в зависимости от расположения на фланге.
    /// </summary>
    private static bool CanAttackOnFlank(
        UnitSquadFlankPosition currentUnitFlankPosition,
        UnitSquadFlankPosition targetUnitFlankPosition,
        UnitSquadLinePosition targetUnitLinePosition,
        Squad targetSquad)
    {
        // Если юниты находятся по разные стороны флагов и занят вражеский центр или соседняя с атакующим клетка, то атаковать нельзя.
        if (Math.Abs(currentUnitFlankPosition - targetUnitFlankPosition) > 1 &&
            (!IsPlaceEmpty(targetSquad, targetUnitLinePosition, UnitSquadFlankPosition.Center)
             || !IsPlaceEmpty(targetSquad, targetUnitLinePosition, currentUnitFlankPosition)))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Проверить, свободна ли клетка на арене.
    /// </summary>
    private static bool IsPlaceEmpty(Squad squad, UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition)
    {
        return squad.Units.Any(u => u.SquadLinePosition == linePosition &&
                                    u.SquadFlankPosition == flankPosition &&
                                    u.IsDeadOrRetreated == false) == false;
    }

    #endregion
}