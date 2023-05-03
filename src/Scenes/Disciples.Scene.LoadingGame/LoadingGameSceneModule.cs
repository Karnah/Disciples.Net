using Disciples.Engine.Base;
using DryIoc;

namespace Disciples.Scene.LoadingGame;

/// <summary>
/// Модуль регистрации для сцены загрузки.
/// </summary>
public class LoadingGameSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var loadingScopeReuse = new CurrentScopeReuse(nameof(ILoadingGameScene));
        containerRegistrator.Register<ILoadingGameScene, LoadingGameScene>(loadingScopeReuse);
    }
}