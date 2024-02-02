using System.Collections.Generic;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Отряд.
/// </summary>
public class Squad
{
    /// <summary>
    /// Создать объект типа <see cref="Squad" />.
    /// </summary>
    /// <param name="player">Игрок, которому принадлежит отряд.</param>
    /// <param name="units">Юниты в отряде.</param>
    public Squad(Player player, List<Unit> units)
    {
        Player = player;
        Units = units;
    }

    /// <summary>
    /// Игрок, которому принадлежит отряд.
    /// </summary>
    public Player Player { get; }

    /// <summary>
    /// Юниты в отряде.
    /// </summary>
    public List<Unit> Units { get; }
}