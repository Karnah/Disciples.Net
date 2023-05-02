using System.Collections.Generic;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common;

/// <summary>
/// Данные игры.
/// </summary>
public class GameContext
{
    /// <summary>
    /// Игроки.
    /// </summary>
    public IReadOnlyList<Player> Players { get; init; } = new List<Player>();
}