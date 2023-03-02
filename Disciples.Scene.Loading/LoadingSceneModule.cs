using Disciples.Engine.Base;
using DryIoc;

namespace Disciples.Scene.Loading;

/// <summary>
/// Модуль регистрации для сцены загрузки.
/// </summary>
public class LoadingSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var loadingScopeReuse = new CurrentScopeReuse(nameof(ILoadingScene));
        containerRegistrator.Register<ILoadingScene, LoadingScene>(loadingScopeReuse);
    }
}