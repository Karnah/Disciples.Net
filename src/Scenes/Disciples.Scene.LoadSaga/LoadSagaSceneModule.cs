using DryIoc;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Scenes;
using Disciples.Scene.LoadSaga.Controllers;

namespace Disciples.Scene.LoadSaga;

/// <summary>
/// Модуль загрузки сцены сейва из саги <see cref="LoadSagaScene" />.
/// </summary>
public class LoadSagaSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var sceneScopeReuse = new CurrentScopeReuse(nameof(ILoadSagaScene));
        containerRegistrator.Register<ILoadSagaScene, LoadSagaScene>(sceneScopeReuse);
        containerRegistrator.Register<ISceneInterfaceController, SceneInterfaceController>(sceneScopeReuse);
        containerRegistrator.Register<LoadSagaInterfaceController>(sceneScopeReuse);
    }
}