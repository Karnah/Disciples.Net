using Disciples.Engine.Base;
using Disciples.Engine.Scenes;
using DryIoc;

namespace Disciples.Scene.LoadingGame;

/// <summary>
/// Модуль регистрации для сцены загрузки игры.
/// </summary>
public class LoadingGameSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var sceneScopeReuse = new CurrentScopeReuse(nameof(ILoadingGameScene));
        containerRegistrator.Register<ILoadingGameScene, LoadingGameScene>(sceneScopeReuse);
    }
}