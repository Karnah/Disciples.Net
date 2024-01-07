using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Scenes.Parameters;

namespace Disciples.Scene.Video;

/// <inheritdoc cref="IVideoScene" />
internal class VideoScene : BaseScene, IVideoScene
{
    private readonly IGameController _gameController;

    private readonly IReadOnlyList<string> _videoPaths;
    private readonly Action<IGameController> _onCompleted;

    private int? _videoIndex;
    private VideoGameObject _videoObject = null!;

    /// <summary>
    /// Создать объект типа <see cref="VideoScene" />.
    /// </summary>
    public VideoScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        IGameController gameController,
        VideoSceneParameters parameters
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController)
    {
        _gameController = gameController;

        _videoPaths = parameters.VideoPaths;
        _onCompleted = parameters.OnCompleted;
    }

    /// <inheritdoc />
    public override CursorType DefaultCursorType => CursorType.None;

    /// <inheritdoc />
    /// <remarks>
    /// При нажатии даже за пределами экрана, нужно обрабатывать событие на видеоролике.
    /// </remarks>
    protected override GameObject MainInputGameObject => _videoObject;

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        ShowNextVideo();
    }

    /// <inheritdoc />
    protected override void BeforeSceneUpdate(UpdateSceneData data)
    {
        base.BeforeSceneUpdate(data);

        if (IsLoaded && _videoObject.IsCompleted)
            ShowNextVideo();
    }

    /// <summary>
    /// Отобразить следующее видео.
    /// </summary>
    private void ShowNextVideo()
    {
        if (_videoIndex == null)
            _videoIndex = 0;
        else
            _videoIndex++;

        if (_videoPaths.Count <= _videoIndex!.Value)
        {
            _onCompleted.Invoke(_gameController);
            return;
        }

        var nextVideoPath = _videoPaths[_videoIndex.Value];
        if (!File.Exists(nextVideoPath))
        {
            ShowNextVideo();
            return;
        }

        var fileStream = File.OpenRead(nextVideoPath);
        _videoObject = GameObjectContainer.AddVideo(
            fileStream,
            GameInfo.SceneBounds,
            Layers.SceneLayers.BackgroundLayer);
    }
}