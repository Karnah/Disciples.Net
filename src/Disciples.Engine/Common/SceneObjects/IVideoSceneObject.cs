namespace Disciples.Engine.Common.SceneObjects;

/// <summary>
/// Объект для воспроизведения видео на сцене.
/// </summary>
public interface IVideoSceneObject : ISceneObject
{
    /// <summary>
    /// Признак, что видео закончило воспроизведение.
    /// </summary>
    bool IsCompleted { get; }
}