using Disciples.Engine.Models;

namespace Disciples.Engine.Scenes.Parameters;

/// <summary>
/// Параметры, необходимые для инициализации сцены загрузки сейва <see cref="ILoadingSaveScene" />.
/// </summary>
public class LoadingSaveSceneParameters : SceneParameters
{
    /// <summary>
    /// Создать объект типа <see cref="LoadingSaveSceneParameters" />.
    /// </summary>
    public LoadingSaveSceneParameters(string savePath)
    {
        SavePath = savePath;
    }

    /// <summary>
    /// Путь до сейва.
    /// </summary>
    public string SavePath { get; }
}