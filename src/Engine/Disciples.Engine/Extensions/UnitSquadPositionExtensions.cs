using System.Collections.Generic;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Extensions;

/// <summary>
/// Методы для работы с позицией юнита.
/// </summary>
public static class UnitSquadPositionExtensions
{
    /// <summary>
    /// Возможные позиции для юнита.
    /// </summary>
    public static readonly IReadOnlyList<UnitSquadPosition> Positions = new[]
    {
        UnitSquadPosition.BackBottom,
        UnitSquadPosition.BackCenter,
        UnitSquadPosition.BackTop,
        UnitSquadPosition.FrontBottom,
        UnitSquadPosition.FrontCenter,
        UnitSquadPosition.FrontTop,
    };

    /// <summary>
    /// Проверить, пересекаются ли две позиции.
    /// </summary>
    public static bool IsIntersect(this UnitSquadPosition position, UnitSquadPosition otherPosition)
    {
        var intersection = position & otherPosition;
        var isLineIntersect = intersection.HasFlag(UnitSquadPosition.Back) ||
                              intersection.HasFlag(UnitSquadPosition.Front);
        var isFlankIntersect = intersection.HasFlag(UnitSquadPosition.Bottom) ||
                               intersection.HasFlag(UnitSquadPosition.Center) ||
                               intersection.HasFlag(UnitSquadPosition.Top);
        return isLineIntersect && isFlankIntersect;
    }

    /// <summary>
    /// Получить позицию юнита в отряде.
    /// </summary>
    public static UnitSquadPosition GetPosition(bool isSmall, UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition)
    {
        var line = isSmall
            ? 1 << (int)linePosition
            : (int)UnitSquadPosition.Big;
        var flank = 1 << ((int)flankPosition + 5);
        return (UnitSquadPosition) (line | flank);
    }

    /// <summary>
    /// Получить позицию юнита в отряде.
    /// </summary>
    public static (UnitSquadLinePosition LinePosition, UnitSquadFlankPosition FlankPosition) GetPosition(this UnitSquadPosition position)
    {
        var linePosition = position.HasFlag(UnitSquadPosition.Front)
            ? UnitSquadLinePosition.Front
            : UnitSquadLinePosition.Back;
        var flankPosition = position.HasFlag(UnitSquadPosition.Top)
            ? UnitSquadFlankPosition.Top
            : position.HasFlag(UnitSquadPosition.Center)
                ? UnitSquadFlankPosition.Center
                : UnitSquadFlankPosition.Bottom;
        return (linePosition, flankPosition);
    }

    /// <summary>
    /// Получить линию.
    /// </summary>
    public static UnitSquadPosition GetLine(this UnitSquadPosition position)
    {
        return position & UnitSquadPosition.HorizontalLine;
    }

    /// <summary>
    /// Получить позицию по вертикали.
    /// </summary>
    public static UnitSquadPosition GetFlank(this UnitSquadPosition position)
    {
        return position & UnitSquadPosition.VerticalLine;
    }

    /// <summary>
    /// Получить ту же позицию на другой линии.
    /// </summary>
    public static UnitSquadPosition GetOtherLine(this UnitSquadPosition position)
    {
        return position ^ UnitSquadPosition.HorizontalLine;
    }
}
