using Disciples.Engine.Common;
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
    public LoadingSaveSceneParameters(GameContext save)
    {
        Save = save;
    }

    /// <summary>
    /// Данные сейва.
    /// </summary>
    public GameContext Save { get; }
}