using Disciples.Common.Models;

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
    public Bounds Bounds { get; init; }

    /// <summary>
    /// Название фона.
    /// </summary>
    public string? BackgroundImageName { get; init; }

    /// <summary>
    /// Название изображения с курсором.
    /// </summary>
    public string? CursorImageName { get; init; }

    /// <summary>
    /// Что-то магическое, много значений 0.
    /// </summary>
    public int Magic { get; init; }

    /// <summary>
    /// Расположение элемента на экране.
    /// </summary>
    public Bounds Position { get; init; }

    /// <summary>
    /// Что-то магическое, много значений 0.
    /// </summary>
    public int Magic2 { get; init; }

    /// <summary>
    /// Элементы интерфейса.
    /// </summary>
    public IReadOnlyList<SceneElement> Elements { get; init; } = Array.Empty<SceneElement>();
}