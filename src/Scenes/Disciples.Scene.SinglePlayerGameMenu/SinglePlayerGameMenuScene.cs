using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.SinglePlayerGameMenu.Controllers;

namespace Disciples.Scene.SinglePlayerGameMenu;

/// <inheritdoc cref="ISinglePlayerGameMenuScene" />
internal class SinglePlayerGameMenuScene : BaseScene, ISinglePlayerGameMenuScene
{
    private readonly SinglePlayerGameMenuInterfaceController _interfaceController;
    private readonly MenuSoundController _soundController;

    /// <summary>
    /// Создать объект типа <see cref="SinglePlayerGameMenuScene" />.
    /// </summary>
    public SinglePlayerGameMenuScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        SinglePlayerGameMenuInterfaceController interfaceController,
        MenuSoundController soundController
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
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
        _interfaceController.Load();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        base.UnloadInternal();

        _soundController.Unload();
        _interfaceController.Unload();
    }

    /// <inheritdoc />
    protected override void BeforeSceneUpdate(UpdateSceneData data)
    {
        _soundController.BeforeSceneUpdate();
    }

    /// <inheritdoc />
    protected override void AfterSceneUpdate()
    {
        _soundController.AfterSceneUpdate();
    }
}