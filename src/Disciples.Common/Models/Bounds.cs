using System;

namespace Disciples.Common.Models;

/// <summary>
/// Границы объекта.
/// </summary>
public readonly record struct Bounds
{
    /// <summary>
    /// Создать границы с указанным размером.
    /// </summary>
    public Bounds(int width, int height) : this()
    {
        Top = height;
        Right = width;
    }

    /// <summary>
    /// Создать границы по координатам.
    /// </summary>
    public Bounds(int bottom, int top, int left, int right)
    {
        Bottom = bottom;
        Top = top;
        Left = left;
        Right = right;
    }

    /// <summary>
    /// Нижняя точка границы.
    /// </summary>
    public int Bottom { get; init; }

    /// <summary>
    /// Верхняя точка границы.
    /// </summary>
    public int Top { get; init; }

    /// <summary>
    /// Самая левая точка границы.
    /// </summary>
    public int Left { get; init; }

    /// <summary>
    /// Самая правая точка границы.
    /// </summary>
    public int Right { get; init; }

    /// <summary>
    /// Ширина изображения.
    /// </summary>
    public int Width => Math.Abs(Right - Left);

    /// <summary>
    /// Высота изображения.
    /// </summary>
    public int Height => Math.Abs(Top - Bottom);
}