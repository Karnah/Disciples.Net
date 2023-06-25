using Disciples.Engine.Base;
using Disciples.Engine.Scenes;
using DryIoc;

namespace Disciples.Scene.Video;

/// <summary>
/// Модуль сцены отображения видео <see cref="VideoScene" />.
/// </summary>
public class VideoSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var sceneScopeReuse = new CurrentScopeReuse(nameof(IVideoScene));
        containerRegistrator.Register<IVideoScene, VideoScene>(sceneScopeReuse);
    }
}