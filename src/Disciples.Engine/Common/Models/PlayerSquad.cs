using System.Collections.Generic;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Отряд игрока.
/// </summary>
public class PlayerSquad
{
    /// <summary>
    /// Список юнитов в отряде.
    /// </summary>
    public List<SquadUnit> Units { get; init; } = new();
}