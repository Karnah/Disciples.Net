using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;
using Disciples.Engine.Settings;
using Disciples.Scene.MainMenu.Constants;

namespace Disciples.Scene.MainMenu.Controllers;

/// <summary>
/// Контроллер интерфейса сцены главного меню.
/// </summary>
internal class MainMenuInterfaceController : BaseSupportLoading
{
    private readonly GameSettings _gameSettings;
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;
    private readonly GameController _gameController;
    private readonly IVideoProvider _videoProvider;
    private readonly MenuSoundController _menuSoundController;

    /// <summary>
    /// Создать объект типа <see cref="MainMenuInterfaceController" />.
    /// </summary>
    public MainMenuInterfaceController(
        GameSettings gameSettings,
        IInterfaceProvider interfaceProvider,
        ISceneInterfaceController sceneInterfaceController,
        GameController gameController,
        IVideoProvider videoProvider,
        MenuSoundController menuSoundController)
    {
        _gameSettings = gameSettings;
        _interfaceProvider = interfaceProvider;
        _sceneInterfaceController = sceneInterfaceController;
        _gameController = gameController;
        _videoProvider = videoProvider;
        _menuSoundController = menuSoundController;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var sceneInterface = _interfaceProvider.GetSceneInterface("DLG_MAIN_MENU");
        var gameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.SceneLayers);

        gameObjects.GetButton(MainMenuElementNames.SINGLE_PLAYER_GAME_BUTTON, ExecuteSinglePlayerGame);
        gameObjects.GetButton(MainMenuElementNames.MULTI_PLAYER_GAME_BUTTON, ExecuteMultiPlayerGame, _gameSettings.IsUselessButtonsHidden);
        gameObjects.GetButton(MainMenuElementNames.TUTORIAL_BUTTON, ExecuteTutorial, _gameSettings.IsUselessButtonsHidden);
        gameObjects.GetButton(MainMenuElementNames.SETTINGS_BUTTON, ExecuteSettings, _gameSettings.IsUselessButtonsHidden);
        gameObjects.GetButton(MainMenuElementNames.INTRO_BUTTON, ExecuteIntro);
        gameObjects.GetButton(MainMenuElementNames.CREDITS_BUTTON, ExecuteCredits, _gameSettings.IsUselessButtonsHidden);
        gameObjects.GetButton(MainMenuElementNames.QUIT_BUTTON, ExecuteQuit);

        gameObjects.GetTextBlock(MainMenuElementNames.VERSION_TEXT_BLOCK,
            new TextContainer(_gameController.Version ?? string.Empty));

        if (_gameSettings.IsUselessButtonsHidden)
        {
            gameObjects.Get<TextBlockObject>("TXT_CREDITS", true);
            gameObjects.Get<TextBlockObject>("TXT_MULTI", true);
            gameObjects.Get<TextBlockObject>("TXT_OPTIONS", true);
            gameObjects.Get<TextBlockObject>("TXT_TUTORIAL", true);
        }
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Перейти в меню одиночной игры.
    /// </summary>
    private void ExecuteSinglePlayerGame()
    {
        _gameController.ChangeScene<ISinglePlayerGameMenuScene, SceneParameters>(SceneParameters.Empty);
    }

    /// <summary>
    /// Выбрать многопользовательскую игру.
    /// </summary>
    private void ExecuteMultiPlayerGame()
    {
    }

    /// <summary>
    /// Начать обучающий сценарий.
    /// </summary>
    private void ExecuteTutorial()
    {

    }

    /// <summary>
    /// Открыть настройки.
    /// </summary>
    private void ExecuteSettings()
    {

    }

    /// <summary>
    /// Показать вступительный ролик.
    /// </summary>
    private void ExecuteIntro()
    {
        _menuSoundController.Stop();

        var videoSceneParameters = new VideoSceneParameters
        {
            VideoPaths = _videoProvider.IntroVideoPaths,
            OnCompleted = gc => gc.ChangeScene<IMainMenuScene, SceneParameters>(SceneParameters.Empty)
        };
        _gameController.ChangeScene<IVideoScene, VideoSceneParameters>(videoSceneParameters);
    }

    /// <summary>
    /// Показать ролик с разработчиками.
    /// </summary>
    private void ExecuteCredits()
    {

    }

    /// <summary>
    /// Покинуть игру.
    /// </summary>
    private void ExecuteQuit()
    {
        _gameController.Quit();
    }
}