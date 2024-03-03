using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;

namespace Disciples.Scene.LoadingGame;

/// <inheritdoc cref="ILoadingGameScene" />
internal class LoadingGameScene : BaseScene, ILoadingGameScene
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly IGameController _gameController;
    private readonly InterfaceImagesExtractor _interfaceImagesExtractor;
    private readonly IRaceProvider _raceProvider;

    /// <summary>
    /// Наименование картинки, которая содержит фон загрузки.
    /// </summary>
    private const string LOADING_IMAGE_NAME = "LOADING";

    private IImageSceneObject? _loadingSceneObject;

    /// <summary>
    /// Создать объект типа <see cref="LoadingGameScene" />.
    /// </summary>
    public LoadingGameScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        IInterfaceProvider interfaceProvider,
        IGameController gameController,
        InterfaceImagesExtractor interfaceImagesExtractor,
        IRaceProvider raceProvider
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _interfaceProvider = interfaceProvider;
        _gameController = gameController;
        _interfaceImagesExtractor = interfaceImagesExtractor;
        _raceProvider = raceProvider;
    }

    /// <inheritdoc />
    public override CursorType DefaultCursorType => CursorType.None;

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        // IInterfaceProvider.GetImage внутри использует только InterfaceImagesExtractor.
        // Загружаем в начале только его. Остальные зависимости будут дозагружены в LoadResources.
        _interfaceImagesExtractor.Load();

        var loadingImage = _interfaceProvider.GetImage(LOADING_IMAGE_NAME);
        _loadingSceneObject = _sceneObjectContainer.AddImage(loadingImage, 0, 0, 0);
    }

    /// <inheritdoc />
    protected override void AfterSceneLoadedInternal()
    {
        base.AfterSceneLoadedInternal();

        Task.Run(LoadGame);
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        base.UnloadInternal();

        _loadingSceneObject?.Destroy();
        _loadingSceneObject = null;
    }

    /// <summary>
    /// Загрузить игру и её ресурсы.
    /// </summary>
    private void LoadGame()
    {
        LoadResources();

        _gameController.ChangeScene<IMainMenuScene, SceneParameters>(SceneParameters.Empty);
    }

    /// <summary>
    /// Загрузить необходимые ресурсы.
    /// </summary>
    private void LoadResources()
    {
        _interfaceProvider.Load();
        _raceProvider.Load();
    }
}