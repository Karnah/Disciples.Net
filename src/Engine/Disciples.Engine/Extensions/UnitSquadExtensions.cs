using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Extensions;

/// <summary>
/// Методы-расширения для отряда.
/// </summary>
public static class UnitSquadExtensions
{
        /// <summary>
    /// Проверить, что позиция в отряде занята.
    /// </summary>
    public static bool IsPositionBusy(this Squad squad, UnitSquadPosition position)
    {
        return squad.IsPositionBusy(position.Line, position.Flank);
    }

    /// <summary>
    /// Проверить, что позиция в отряде занята.
    /// </summary>
    public static bool IsPositionBusy(this Squad squad, UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition)
    {
        return squad.GetUnits(linePosition, flankPosition).Any();
    }

    /// <summary>
    /// Проверить, что позиция в отряде занята.
    /// </summary>
    public static bool IsPositionBusy(this Squad squad, UnitSquadPosition position, Func<Unit, bool> filter)
    {
        return squad.IsPositionBusy(position.Line, position.Flank, filter);
    }

    /// <summary>
    /// Проверить, что позиция в отряде занята.
    /// </summary>
    public static bool IsPositionBusy(this Squad squad, UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition, Func<Unit, bool> filter)
    {
        return squad.GetUnits(linePosition, flankPosition).Where(filter).Any();
    }

    /// <summary>
    /// Проверить, что свободна позиция в отряде.
    /// </summary>
    public static bool IsPositionEmpty(this Squad squad, UnitSquadPosition position)
    {
        return squad.IsPositionEmpty(position.Line, position.Flank);
    }

    /// <summary>
    /// Проверить, что свободна позиция в отряде.
    /// </summary>
    public static bool IsPositionEmpty(this Squad squad, UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition)
    {
        return !squad.IsPositionBusy(linePosition, flankPosition);
    }

    /// <summary>
    /// Проверить, что свободна позиция в отряде.
    /// </summary>
    public static bool IsPositionEmpty(this Squad squad, UnitSquadPosition position, Func<Unit, bool> filter)
    {
        return squad.IsPositionEmpty(position.Line, position.Flank, filter);
    }

    /// <summary>
    /// Проверить, что свободна позиция в отряде.
    /// </summary>
    public static bool IsPositionEmpty(this Squad squad, UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition, Func<Unit, bool> filter)
    {
        return !squad.IsPositionBusy(linePosition, flankPosition, filter);
    }

    /// <summary>
    /// Получить юнитов на указанной позиции.
    /// </summary>
    public static IEnumerable<Unit> GetUnits(this Squad squad, UnitSquadPosition position)
    {
        return squad.GetUnits(position.Line, position.Flank);
    }

    /// <summary>
    /// Получить юнитов на указанной позиции.
    /// </summary>
    public static IEnumerable<Unit> GetUnits(this Squad squad, UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition)
    {
        return squad
            .Units
            .Where(u => u.Position.Line.IsIntersect(linePosition) && u.Position.Flank == flankPosition);
    }
}
