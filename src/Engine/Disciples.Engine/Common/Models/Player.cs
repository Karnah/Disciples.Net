using System.Collections.Generic;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Информация об игроке.
/// </summary>
public class Player
{
    /// <summary>
    /// Уникальный идентификатор игрока.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Управляется ли игрок компьютером (ИИ).
    /// </summary>
    public bool IsComputer { get; init; }

    /// <summary>
    /// Раса игрока.
    /// </summary>
    public RaceType Race { get; init; }

    /// <summary>
    /// Отряды игрока.
    /// </summary>
    public List<PlayerSquad> Squads { get; init; } = new();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Player)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id;
    }

    /// <summary>
    /// Оператор равенства.
    /// </summary>
    public static bool operator ==(Player? left, Player? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Оператор неравенства.
    /// </summary>
    public static bool operator !=(Player? left, Player? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Сравнить элементы.
    /// </summary>
    private bool Equals(Player other)
    {
        return Id == other.Id;
    }
}