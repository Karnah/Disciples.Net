using Disciples.Engine.Base;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;

namespace Disciples.Scene.Loading;

/// <inheritdoc cref="ILoadingScene" />
internal class LoadingScene : BaseScene, ILoadingScene
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IInterfaceProvider _interfaceProvider;

    /// <summary>
    /// Наименование картинки, которая содержит фон загрузки.
    /// </summary>
    private const string LOADING_IMAGE_NAME = "LOADING";

    private IImageSceneObject? _loadingSceneObject;

    /// <summary>
    /// Создать объект типа <see cref="LoadingScene" />.
    /// </summary>
    public LoadingScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IInterfaceProvider interfaceProvider
        ) : base(gameObjectContainer, sceneObjectContainer)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _interfaceProvider = interfaceProvider;
    }

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
    protected override void UnloadInternal()
    {
        base.UnloadInternal();

        if (!_interfaceProvider.IsSharedBetweenScenes)
            _interfaceProvider.Load();

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
}