using System.Collections.Generic;
using System.IO;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <summary>
/// Базовый контейнер игровых объектов для сцены.
/// </summary>
/// <remarks>
/// Используется обёртка над <see cref="GameObjectContainer" />, чтобы не было проблемы с DI.
/// </remarks>
public abstract class BaseSceneGameObjectContainer : IGameObjectContainer
{
    /// <summary>
    /// Создать объект типа <see cref="BaseSceneGameObjectContainer" />.
    /// </summary>
    protected BaseSceneGameObjectContainer(IGameObjectContainer gameObjectContainer)
    {
        GameObjectContainer = gameObjectContainer;
    }

    /// <summary>
    /// Общий класс контейнера.
    /// </summary>
    protected IGameObjectContainer GameObjectContainer { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<GameObject> GameObjects => GameObjectContainer.GameObjects;

    /// <inheritdoc />
    public TextBlockObject AddTextBlock(TextBlockSceneElement textBlock, int layer)
    {
        return GameObjectContainer.AddTextBlock(textBlock, layer);
    }

    /// <inheritdoc />
    public ImageObject AddImage(ImageSceneElement image, int layer)
    {
        return GameObjectContainer.AddImage(image, layer);
    }

    /// <inheritdoc />
    public AnimationObject AddAnimation(AnimationFrames frames, double x, double y, int layer, bool repeat = true)
    {
        return GameObjectContainer.AddAnimation(frames, x, y, layer, repeat);
    }

    /// <inheritdoc />
    public AnimationObject AddAnimation(AnimationSceneElement animation, int layer, bool repeat = true)
    {
        return GameObjectContainer.AddAnimation(animation, layer, repeat);
    }

    /// <inheritdoc />
    public ButtonObject AddButton(ButtonSceneElement button, int layer)
    {
        return GameObjectContainer.AddButton(button, layer);
    }

    /// <inheritdoc />
    public ToggleButtonObject AddToggleButton(ToggleButtonSceneElement toggleButton, int layer)
    {
        return GameObjectContainer.AddToggleButton(toggleButton, layer);
    }

    /// <inheritdoc />
    public TextListBoxObject AddTextListBox(TextListBoxSceneElement textListBox, int layer)
    {
        return GameObjectContainer.AddTextListBox(textListBox, layer);
    }

    /// <inheritdoc />
    public VideoGameObject AddVideo(Stream videoStream, RectangleD bounds, int layer, bool canSkip = true)
    {
        return GameObjectContainer.AddVideo(videoStream, bounds, layer, canSkip);
    }

    /// <inheritdoc />
    public TGameObject AddObject<TGameObject>(TGameObject gameObject) where TGameObject : GameObject
    {
        return GameObjectContainer.AddObject(gameObject);
    }

    /// <inheritdoc />
    public void UpdateGameObjects(long ticksCount)
    {
        GameObjectContainer.UpdateGameObjects(ticksCount);
    }
}