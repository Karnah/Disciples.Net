﻿using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
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
    public static bool CanAttackEnemy(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        if (context.CurrentUnit.Player.Id == context.TargetUnit.Player.Id)
            return false;

        if (context.TargetUnit.IsInactive)
            return false;

        if (!CanAttackPosition(context.CurrentUnit, context.TargetUnit, context.CurrentUnitSquad, context.TargetUnitSquad, unitAttack))
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

        if (context.TargetUnit.IsInactive)
            return false;

        return true;
    }

    #endregion

    #region Проверка позиции для атаки

    /// <summary>
    /// Проверить можно ли провести атаку с учётом позиций юнита.
    /// </summary>
    private static bool CanAttackPosition(Unit attackingUnit, Unit targetUnit, Squad attackingSquad, Squad targetSquad,
        CalculatedUnitAttack unitAttack)
    {
        if (unitAttack.Reach is UnitAttackReach.All or UnitAttackReach.Any)
            return true;

        // Атакующий юнит сзади, впереди мешает линия союзников.
        if (attackingUnit.Position.Line == UnitSquadLinePosition.Back && !IsFrontLineEmpty(attackingSquad))
            return false;

        // Цель находится на задней линии, но на передней линии есть юниты.
        if (targetUnit.Position.Line == UnitSquadLinePosition.Back && !IsFrontLineEmpty(targetSquad))
            return false;

        // Проверка, может ли юнит дотянуться до врага.
        if (!CanAttackOnFlank(attackingUnit.Position, targetUnit.Position, targetSquad))
            return false;

        return true;
    }

    /// <summary>
    /// Проверить, свободна ли первая линия в отряде.
    /// </summary>
    private static bool IsFrontLineEmpty(Squad squad)
    {
        return !squad.Units.Any(u => u.Position.Line.HasFlag(UnitSquadLinePosition.Front) && !u.IsInactive);
    }

    /// <summary>
    /// Проверить, можно ли атаковать цель в зависимости от расположения на фланге.
    /// </summary>
    private static bool CanAttackOnFlank(
        UnitSquadPosition currentUnitPosition,
        UnitSquadPosition targetUnitPosition,
        Squad targetSquad)
    {
        var line = targetUnitPosition.Line == UnitSquadLinePosition.Both
            ? UnitSquadLinePosition.Front
            : targetUnitPosition.Line;

        // Если юниты находятся по разные стороны флагов и занят вражеский центр или соседняя с атакующим клетка, то атаковать нельзя.
        if (Math.Abs(currentUnitPosition.Flank - targetUnitPosition.Flank) > 1 &&
            (targetSquad.IsPositionBusy(line, UnitSquadFlankPosition.Center, u => !u.IsInactive) ||
             targetSquad.IsPositionBusy(line, currentUnitPosition.Flank, u => !u.IsInactive)))
        {
            return false;
        }

        return true;
    }

    #endregion
}