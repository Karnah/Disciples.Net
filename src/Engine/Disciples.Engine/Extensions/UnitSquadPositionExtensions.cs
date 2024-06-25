using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Extensions;

/// <summary>
/// Методы для работы с позицией юнита.
/// </summary>
public static class UnitSquadPositionExtensions
{
    /// <summary>
    /// Все возможные позиции для маленьких юнитов в отряде.
    /// </summary>
    public static IReadOnlyList<UnitSquadPosition> Positions { get; } = GetPositions();

    /// <summary>
    /// Проверить, пересекаются ли две позиции.
    /// </summary>
    public static bool IsIntersect(this UnitSquadPosition squadPosition, UnitSquadPosition otherSquadPosition)
    {
        return squadPosition.Line.IsIntersect(otherSquadPosition.Line) &&
               squadPosition.Flank == otherSquadPosition.Flank;
    }

    /// <summary>
    /// Проверить, пересекаются ли две позиции.
    /// </summary>
    public static bool IsIntersect(this UnitSquadLinePosition linePosition, UnitSquadLinePosition otherLinePosition)
    {
        return (linePosition & otherLinePosition) != UnitSquadLinePosition.None;
    }

    /// <summary>
    /// Превратить линию в позицию смещения.
    /// </summary>
    public static int ToIndex(this UnitSquadLinePosition linePosition)
    {
        return linePosition switch
        {
            UnitSquadLinePosition.Back => 0,
            UnitSquadLinePosition.Front => 1,
            // Большие юниты считаются как юниты первой линии.
            UnitSquadLinePosition.Both => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(linePosition), linePosition, null)
        };
    }

    /// <summary>
    /// Превратить линию в позицию смещения в обратном порядке.
    /// </summary>
    public static int ToReverseIndex(this UnitSquadLinePosition linePosition)
    {
        return 1 - linePosition.ToIndex();
    }

    /// <summary>
    /// Превратить положение в позицию смещения.
    /// </summary>
    public static int ToIndex(this UnitSquadFlankPosition flankPosition)
    {
        return (int)flankPosition;
    }

    /// <summary>
    /// Превратить положение в позицию смещения в обратном порядке.
    /// </summary>
    public static int ToReverseIndex(this UnitSquadFlankPosition flankPosition)
    {
        return 2 - flankPosition.ToIndex();
    }

    /// <summary>
    /// Получить позицию на противоположной линии.
    /// </summary>
    public static UnitSquadPosition GetOtherLine(this UnitSquadPosition position)
    {
        return new UnitSquadPosition(position.Line.GetOtherLine(), position.Flank);
    }

    /// <summary>
    /// Получить противоположную линию.
    /// </summary>
    public static UnitSquadLinePosition GetOtherLine(this UnitSquadLinePosition linePosition)
    {
        return linePosition switch
        {
            UnitSquadLinePosition.Back => UnitSquadLinePosition.Front,
            UnitSquadLinePosition.Front => UnitSquadLinePosition.Back,
            UnitSquadLinePosition.Both => UnitSquadLinePosition.Both,
            _ => throw new ArgumentOutOfRangeException(nameof(linePosition), linePosition, null)
        };
    }

    /// <summary>
    /// Получить все возможные позиции для маленьких юнитов в отряде.
    /// </summary>
    private static IReadOnlyList<UnitSquadPosition> GetPositions()
    {
        var positions = new List<UnitSquadPosition>(6);
        foreach (var line in new[] { UnitSquadLinePosition.Back, UnitSquadLinePosition.Front })
        foreach (var flank in new [] { UnitSquadFlankPosition.Bottom, UnitSquadFlankPosition.Center, UnitSquadFlankPosition.Top })
            positions.Add(new UnitSquadPosition(line, flank));
        return positions;
    }
}
