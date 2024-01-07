using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.MainMenu.Controllers;

namespace Disciples.Scene.MainMenu;

/// <inheritdoc cref="IMainMenuScene" />
internal class MainMenuScene : BaseMenuScene, IMainMenuScene
{
    private readonly MainMenuInterfaceController _interfaceController;
    private readonly MenuSoundController _soundController;

    /// <summary>
    /// Создать объект типа <see cref="MainMenuScene" />.
    /// </summary>
    public MainMenuScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        IInterfaceProvider interfaceProvider,
        MainMenuInterfaceController interfaceController,
        MenuSoundController soundController
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController, interfaceProvider)
    {
        _interfaceController = interfaceController;
        _soundController = soundController;
    }

    /// <inheritdoc />
    protected override string TransitionAnimationName => "TRANS_INTRO2MAIN";

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
        base.BeforeSceneUpdate(data);

        _soundController.BeforeSceneUpdate();
    }

    /// <inheritdoc />
    protected override void AfterSceneUpdate()
    {
        base.AfterSceneUpdate();

        _soundController.AfterSceneUpdate();
    }
}