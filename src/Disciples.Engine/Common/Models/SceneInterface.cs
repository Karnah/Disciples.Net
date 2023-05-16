using System.Collections.Generic;
using Disciples.Common.Models;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.Models;

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
    /// Фон.
    /// </summary>
    public IBitmap? Background { get; init; }

    /// <summary>
    /// Название изображения с курсором.
    /// </summary>
    public CursorType CursorType { get; init; }

    /// <summary>
    /// Расположение элемента на экране.
    /// </summary>
    public Bounds Position { get; init; }

    /// <summary>
    /// Элементы интерфейса.
    /// </summary>
    public IReadOnlyDictionary<string, SceneElement> Elements { get; init; } = null!;
}