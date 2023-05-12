using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.LoadSaga.Controllers;
using Disciples.Scene.LoadSaga.Providers;

namespace Disciples.Scene.LoadSaga;

/// <inheritdoc cref="ILoadSagaScene" />
internal class LoadSagaScene : BaseScene, ILoadSagaScene
{
    private readonly LoadSagaInterfaceProvider _interfaceProvider;
    private readonly LoadSagaInterfaceController _interfaceController;
    private readonly LoadSagaSoundController _soundController;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaScene" />.
    /// </summary>
    public LoadSagaScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        LoadSagaInterfaceProvider interfaceProvider,
        LoadSagaInterfaceController interfaceController,
        LoadSagaSoundController soundController
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
        _interfaceProvider = interfaceProvider;
        _interfaceController = interfaceController;
        _soundController = soundController;
    }

    /// <inheritdoc />
    public void InitializeParameters(SceneParameters parameters)
    {
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        _soundController.Load();
        _interfaceProvider.Load();
        _interfaceController.Load();
    }

    protected override void UnloadInternal()
    {
        base.UnloadInternal();

        _soundController.Unload();
    }

    /// <inheritdoc />
    protected override void BeforeSceneUpdate(UpdateSceneData data)
    {
        _soundController.BeforeSceneUpdate();
        _interfaceController.BeforeSceneUpdate();
    }

    /// <inheritdoc />
    protected override void AfterSceneUpdate()
    {
        _soundController.AfterSceneUpdate();
        _interfaceController.AfterSceneUpdate();
    }
}