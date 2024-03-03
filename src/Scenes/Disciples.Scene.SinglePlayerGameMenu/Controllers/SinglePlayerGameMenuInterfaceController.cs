using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Settings;
using Disciples.Scene.SinglePlayerGameMenu.Constants;

namespace Disciples.Scene.SinglePlayerGameMenu.Controllers;

/// <summary>
/// Контроллер интерфейса сцены меню одиночной игры.
/// </summary>
internal class SinglePlayerGameMenuInterfaceController : BaseSupportLoading
{
    private readonly GameSettings _gameSettings;
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;
    private readonly IGameController _gameController;

    /// <summary>
    /// Создать объект типа <see cref="SinglePlayerGameMenuInterfaceController" />.
    /// </summary>
    public SinglePlayerGameMenuInterfaceController(
        GameSettings gameSettings,
        IInterfaceProvider interfaceProvider,
        ISceneInterfaceController sceneInterfaceController,
        IGameController gameController)
    {
        _gameSettings = gameSettings;
        _interfaceProvider = interfaceProvider;
        _sceneInterfaceController = sceneInterfaceController;
        _gameController = gameController;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var sceneInterface = _interfaceProvider.GetSceneInterface("DLG_SINGLE_PLAYER");
        var gameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.SceneLayers);

        gameObjects.GetButton(SinglePlayerGameMenuElementNames.NEW_SAGA_BUTTON, ExecuteNewSaga, _gameSettings.IsUselessButtonsHidden);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.LOAD_SAGA_BUTTON, ExecuteLoadSaga);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.NEW_QUEST_BUTTON, ExecuteNewQuest, _gameSettings.IsUselessButtonsHidden);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.LOAD_QUEST_BUTTON, ExecuteLoadQuest);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.NEW_CUSTOM_SAGA_BUTTON, ExecuteNewCustomSaga, _gameSettings.IsUselessButtonsHidden);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.LOAD_CUSTOM_SAGA_BUTTON, ExecuteLoadCustomSaga, _gameSettings.IsUselessButtonsHidden);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.BACK_BUTTON, ExecuteBack);

        if (_gameSettings.IsUselessButtonsHidden)
        {
            gameObjects.Get<TextBlockObject>("TXT_CUSTOMCAMP", true);
            gameObjects.Get<TextBlockObject>("TXT_LOADCUSTOM", true);
            gameObjects.Get<TextBlockObject>("TXT_NEWQUEST", true);
            gameObjects.Get<TextBlockObject>("TXT_NEWSAGA", true);
        }
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Начать новую сагу.
    /// </summary>
    private void ExecuteNewSaga()
    {

    }

    /// <summary>
    /// Загрузить сагу.
    /// </summary>
    private void ExecuteLoadSaga()
    {
        _gameController.ChangeScene<ILoadSagaScene, SceneParameters>(SceneParameters.Empty);
    }

    /// <summary>
    /// Начать новый квест.
    /// </summary>
    private void ExecuteNewQuest()
    {

    }

    /// <summary>
    /// Загрузить квест.
    /// </summary>
    private void ExecuteLoadQuest()
    {
        _gameController.ChangeScene<ILoadQuestScene, SceneParameters>(SceneParameters.Empty);
    }

    /// <summary>
    /// Начать пользовательскую сагу.
    /// </summary>
    private void ExecuteNewCustomSaga()
    {

    }

    /// <summary>
    /// Загрузить пользовательскую сагу.
    /// </summary>
    private void ExecuteLoadCustomSaga()
    {

    }

    /// <summary>
    /// Вернуться в главное меню.
    /// </summary>
    private void ExecuteBack()
    {
        _gameController.ChangeScene<IMainMenuScene, SceneParameters>(SceneParameters.Empty);
    }
}