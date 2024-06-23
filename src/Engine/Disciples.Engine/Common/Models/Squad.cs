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
    public Squad(Player player)
    {
        Player = player;
        Units = new List<Unit>();
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