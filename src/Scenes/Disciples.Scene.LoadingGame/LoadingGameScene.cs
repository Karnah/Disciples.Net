using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;

namespace Disciples.Scene.LoadingGame;

/// <inheritdoc cref="ILoadingGameScene" />
internal class LoadingGameScene : BaseScene, ILoadingGameScene
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly IGameController _gameController;
    private readonly ITextProvider _textProvider;

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
        ITextProvider textProvider
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _interfaceProvider = interfaceProvider;
        _gameController = gameController;
        _textProvider = textProvider;
    }

    /// <inheritdoc />
    public override CursorState DefaultCursorState => CursorState.None;

    /// <inheritdoc />
    public void InitializeParameters(SceneParameters parameters)
    {
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        _interfaceProvider.Load();

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

    /// <inheritdoc />
    protected override void BeforeSceneUpdate(UpdateSceneData data)
    {
    }

    /// <inheritdoc />
    protected override void AfterSceneUpdate()
    {
    }

    /// <summary>
    /// Загрузить игру и её ресурсы.
    /// </summary>
    private void LoadGame()
    {
        LoadResources();

        _gameController.ChangeScene<ILoadSagaScene, SceneParameters>(SceneParameters.Empty);
    }

    /// <summary>
    /// Загрузить необходимые ресурсы.
    /// </summary>
    private void LoadResources()
    {
        // TODO Добавить загрузку всех ресурсов, необходимых для меню.
        _textProvider.Load();
    }
}