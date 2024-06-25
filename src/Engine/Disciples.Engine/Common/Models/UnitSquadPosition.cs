using System;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Позиция юнита в отряде.
/// </summary>
public readonly struct UnitSquadPosition
{
    /// <summary>
    /// Создать объект типа <see cref="UnitSquadPosition" />.
    /// </summary>
    public UnitSquadPosition(UnitSquadLinePosition line, UnitSquadFlankPosition flank)
    {
        Line = line;
        Flank = flank;
    }

    /// <summary>
    /// Линия.
    /// </summary>
    public UnitSquadLinePosition Line { get; }

    /// <summary>
    /// Фланг.
    /// </summary>
    public UnitSquadFlankPosition Flank { get; }

    /// <summary>
    /// Сравнить две позиции.
    /// </summary>
    public bool Equals(UnitSquadPosition other)
    {
        return Line == other.Line && Flank == other.Flank;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is UnitSquadPosition other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine((int)Line, (int)Flank);
    }

    /// <summary>
    /// Проверить две позиции на равенство.
    /// </summary>
    public static bool operator ==(UnitSquadPosition left, UnitSquadPosition right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Проверить две позиции на неравенство.
    /// </summary>
    public static bool operator !=(UnitSquadPosition left, UnitSquadPosition right)
    {
        return !left.Equals(right);
    }
}
