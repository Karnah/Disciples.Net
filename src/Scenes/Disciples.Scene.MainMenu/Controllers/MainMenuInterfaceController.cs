using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.MainMenu.Constants;

namespace Disciples.Scene.MainMenu.Controllers;

/// <summary>
/// Контроллер интерфейса сцены главного меню.
/// </summary>
internal class MainMenuInterfaceController : BaseSupportLoading
{
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;
    private readonly GameController _gameController;

    private ButtonObject _singlePlayerGameButton = null!;
    private ButtonObject _multiPlayerGameButton = null!;
    private ButtonObject _tutorialButton = null!;
    private ButtonObject _settingsButton = null!;
    private ButtonObject _introButton = null!;
    private ButtonObject _creditsButton = null!;
    private ButtonObject _quitButton = null!;

    private TextBlockObject _versionTextBlock = null!;

    /// <summary>
    /// Создать объект типа <see cref="MainMenuInterfaceController" />.
    /// </summary>
    public MainMenuInterfaceController(
        IInterfaceProvider interfaceProvider,
        ISceneInterfaceController sceneInterfaceController,
        GameController gameController)
    {
        _interfaceProvider = interfaceProvider;
        _sceneInterfaceController = sceneInterfaceController;
        _gameController = gameController;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var sceneInterface = _interfaceProvider.GetSceneInterface("DLG_MAIN_MENU");
        var gameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.SceneLayers);

        _singlePlayerGameButton = gameObjects.GetButton(MainMenuElementNames.SINGLE_PLAYER_GAME_BUTTON, ExecuteSinglePlayerGame);
        _multiPlayerGameButton = gameObjects.GetButton(MainMenuElementNames.MULTI_PLAYER_GAME_BUTTON, ExecuteMultiPlayerGame);
        _tutorialButton = gameObjects.GetButton(MainMenuElementNames.TUTORIAL_BUTTON, ExecuteTutorial);
        _settingsButton = gameObjects.GetButton(MainMenuElementNames.SETTINGS_BUTTON, ExecuteSettings);
        _introButton = gameObjects.GetButton(MainMenuElementNames.INTRO_BUTTON, ExecuteIntro);
        _creditsButton = gameObjects.GetButton(MainMenuElementNames.CREDITS_BUTTON, ExecuteCredits);
        _quitButton = gameObjects.GetButton(MainMenuElementNames.QUIT_BUTTON, ExecuteQuit);

        _versionTextBlock = gameObjects.Get<TextBlockObject>(MainMenuElementNames.VERSION_TEXT_BLOCK);
        _versionTextBlock.Text = new TextContainer(_gameController.Version ?? string.Empty);
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