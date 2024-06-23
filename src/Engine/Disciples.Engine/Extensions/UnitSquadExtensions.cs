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
    /// Получить юнитов на указанной позиции.
    /// </summary>
    public static IEnumerable<Unit> GetUnits(this Squad squad, UnitSquadPosition position)
    {
        return squad
            .Units
            .Where(u => u.SquadPosition.IsIntersect(position));
    }
}
