using System.Drawing;
using Disciples.Resources.Images.Enums;

namespace Disciples.Resources.Images.Models;

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
    public Rectangle Position { get; init; }
}