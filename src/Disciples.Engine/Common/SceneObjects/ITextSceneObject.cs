using Disciples.Engine.Models;

namespace Disciples.Engine.Common.SceneObjects;

/// <summary>
/// Игровой объект, который отображает текст на сцене.
/// </summary>
public interface ITextSceneObject : ISceneObject
{
    /// <summary>
    /// Общий стиль.
    /// </summary>
    TextStyle TextStyle { get; set; }

    /// <summary>
    /// Текст.
    /// </summary>
    TextContainer? Text { get; set; }
}