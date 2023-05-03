using Disciples.Engine.Base;
using Disciples.Engine.Scenes;
using DryIoc;

namespace Disciples.Scene.LoadingSave;

/// <summary>
/// Модуль регистрации для сцены сейва.
/// </summary>
public class LoadingSaveSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var loadingScopeReuse = new CurrentScopeReuse(nameof(ILoadingSaveScene));
        containerRegistrator.Register<ILoadingSaveScene, LoadingSaveScene>(loadingScopeReuse);
    }
}