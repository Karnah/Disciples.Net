using System.IO;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Объект для воспроизведения видео.
/// </summary>
public class VideoGameObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;

    private readonly Stream _videoStream;
    private readonly int _layer;

    private IVideoSceneObject _videoSceneObject = null!;

    /// <summary>
    /// Создать объект типа <see cref="VideoGameObject" />.
    /// </summary>
    public VideoGameObject(ISceneObjectContainer sceneObjectContainer, Stream videoStream, RectangleD position, int layer, bool canSkip) : base(position)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _videoStream = videoStream;
        _layer = layer;

        if (canSkip)
        {
            Components = new IComponent[]
            {
                new SelectionComponent(this, sceneObjectContainer),
                new MouseLeftButtonClickComponent(this, new [] { KeyboardButton.Escape }, Destroy)
            };
        }
    }

    /// <summary>
    /// Признак, что видео закончило воспроизведение.
    /// </summary>
    public bool IsCompleted => _videoSceneObject.IsCompleted;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _videoSceneObject = _sceneObjectContainer.AddVideo(_videoStream, Bounds, _layer);
    }

    /// <inheritdoc />
    public override void Update(long ticksCount)
    {
        base.Update(ticksCount);

        // Автоматически удаляем объект после завершения.
        if (_videoSceneObject.IsCompleted)
            Destroy();
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_videoSceneObject);
    }
}