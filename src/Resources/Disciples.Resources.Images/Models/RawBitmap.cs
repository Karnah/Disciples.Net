using System.Drawing;

namespace Disciples.Resources.Images.Models;

/// <summary>
/// Изображение, представленное массивом пикселей.
/// </summary>
public class RawBitmap
{
    /// <summary>
    /// Ширина всего изображения.
    /// </summary>
    public int OriginalWidth { get; init; }

    /// <summary>
    /// Высота всего изображения.
    /// </summary>
    public int OriginalHeight { get; init; }

    /// <summary>
    /// Границы изображения.
    /// </summary>
    public Rectangle Bounds { get; init; }

    /// <summary>
    /// Массив байт, содержащий пиксели в BGRA.
    /// </summary>
    /// <remarks>Имеет размеры Width * Height * 4.</remarks>
    public byte[] Data { get; init; } = null!;
}