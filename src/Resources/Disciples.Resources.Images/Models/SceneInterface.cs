using System.Drawing;

namespace Disciples.Resources.Images.Models;

/// <summary>
/// Информация об интерфейсе сцены/диалога.
/// </summary>
public class SceneInterface
{
    /// <summary>
    /// Имя.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Размеры.
    /// </summary>
    public Rectangle Bounds { get; init; }

    /// <summary>
    /// Название фона.
    /// </summary>
    public string? BackgroundImageName { get; init; }

    /// <summary>
    /// Название изображения с курсором.
    /// </summary>
    public string? CursorImageName { get; init; }

    /// <summary>
    /// Активная точка курсора.
    /// </summary>
    public Point CursorHotSpot { get; init; }

    /// <summary>
    /// Расположение элемента на экране.
    /// </summary>
    public Rectangle Position { get; init; }

    /// <summary>
    /// Автоматически рисовать интерфейс.
    /// </summary>
    public bool IsSelfDrawn { get; init; }

    /// <summary>
    /// Элементы интерфейса.
    /// </summary>
    public IReadOnlyList<SceneElement> Elements { get; init; } = Array.Empty<SceneElement>();
}