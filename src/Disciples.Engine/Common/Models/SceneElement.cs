using Disciples.Common.Models;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Элемент сцены.
/// </summary>
public abstract class SceneElement
{
    /// <summary>
    /// Тип элемента.
    /// </summary>
    public abstract SceneElementType Type { get; }

    /// <summary>
    /// Название элемента.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Позиция элемента.
    /// </summary>
    public RectangleD Position { get; init; }
}