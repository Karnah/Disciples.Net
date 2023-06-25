using System;
using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Scenes.Parameters;

/// <summary>
/// Параметры для сцены отображения видео <see cref="IVideoScene" />.
/// </summary>
public class VideoSceneParameters : SceneParameters
{
    /// <summary>
    /// Пути для видео, которые необходимо воспроизводить.
    /// </summary>
    public IReadOnlyList<string> VideoPaths { get; init; } = null!;

    /// <summary>
    /// Действие, которое выполняется после завершения воспроизведения.
    /// </summary>
    public Action<IGameController> OnCompleted { get; init; } = null!;
}