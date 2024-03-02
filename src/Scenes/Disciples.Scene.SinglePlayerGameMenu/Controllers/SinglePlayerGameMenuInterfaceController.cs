using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.SinglePlayerGameMenu.Constants;

namespace Disciples.Scene.SinglePlayerGameMenu.Controllers;

/// <summary>
/// Контроллер интерфейса сцены меню одиночной игры.
/// </summary>
internal class SinglePlayerGameMenuInterfaceController : BaseSupportLoading
{
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;
    private readonly IGameController _gameController;

    /// <summary>
    /// Создать объект типа <see cref="SinglePlayerGameMenuInterfaceController" />.
    /// </summary>
    public SinglePlayerGameMenuInterfaceController(
        IInterfaceProvider interfaceProvider,
        ISceneInterfaceController sceneInterfaceController,
        IGameController gameController)
    {
        _interfaceProvider = interfaceProvider;
        _sceneInterfaceController = sceneInterfaceController;
        _gameController = gameController;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var sceneInterface = _interfaceProvider.GetSceneInterface("DLG_SINGLE_PLAYER");
        var gameObjects = _sceneInterfaceController.AddSceneGameObjects(sceneInterface, Layers.SceneLayers);

        gameObjects.GetButton(SinglePlayerGameMenuElementNames.NEW_SAGA_BUTTON, ExecuteNewSaga, true);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.LOAD_SAGA_BUTTON, ExecuteLoadSaga);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.NEW_QUEST_BUTTON, ExecuteNewQuest, true);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.LOAD_QUEST_BUTTON, ExecuteLoadQuest, true);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.NEW_CUSTOM_SAGA_BUTTON, ExecuteNewCustomSaga, true);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.LOAD_CUSTOM_SAGA_BUTTON, ExecuteLoadCustomSaga, true);
        gameObjects.GetButton(SinglePlayerGameMenuElementNames.BACK_BUTTON, ExecuteBack);
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