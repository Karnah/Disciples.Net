using Disciples.Common.Models;

namespace Disciples.Engine;

/// <summary>
/// Информация об игре.
/// </summary>
public static class GameInfo
{
    /// <summary>
    /// Оригинальная ширина экрана.
    /// </summary>
    public const double OriginalWidth = 800;

    /// <summary>
    /// Оригинальная высота экрана.
    /// </summary>
    public const double OriginalHeight = 600;

    /// <summary>
    /// Границы сцены.
    /// </summary>
    public static readonly RectangleD SceneBounds = new RectangleD(0, 0, OriginalWidth, OriginalHeight);


    /// <summary>
    /// Текущая ширина экрана.
    /// </summary>
    public static double Width { get; set; } = 1440;

    /// <summary>
    /// Текущая высота экрана.
    /// </summary>
    public static double Height { get; set; } = 1080;


    /// <summary>
    /// Масштаб экрана относительного базового разрешения.
    /// </summary>
    public static double Scale { get; set; } = Height / OriginalHeight;
}