using System.Collections.Generic;
using System.IO;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Implementation.Base;

/// <inheritdoc cref="ISceneObjectContainer" />
/// <remarks>
/// Класс запечатан, чтобы не было проблем с DI из-за наследования.
/// </remarks>
public class GameObjectContainer : IGameObjectContainer
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly LinkedList<GameObject> _gameObjects;

    /// <summary>
    /// Создать объект типа <see cref="GameObjectContainer" />.
    /// </summary>
    public GameObjectContainer(ISceneObjectContainer sceneObjectContainer)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _gameObjects = new LinkedList<GameObject>();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<GameObject> GameObjects => _gameObjects;

    /// <inheritdoc />
    public TextBlockObject AddTextBlock(TextBlockSceneElement textBlock, int layer)
    {
        var textBlockObject = new TextBlockObject(_sceneObjectContainer, textBlock, layer);
        return AddObject(textBlockObject);
    }

    /// <inheritdoc />
    public ImageObject AddImage(ImageSceneElement image, int layer)
    {
        var imageObject = new ImageObject(_sceneObjectContainer, image, layer);
        return AddObject(imageObject);
    }

    /// <inheritdoc />
    public AnimationObject AddAnimation(AnimationFrames frames, double x, double y, int layer, bool repeat = true)
    {
        var animation = new AnimationObject(_sceneObjectContainer, frames, x, y, layer, repeat);
        return AddObject(animation);
    }

    /// <inheritdoc />
    public AnimationObject AddAnimation(AnimationSceneElement animation, int layer, bool repeat = true)
    {
        var animationObject = new AnimationObject(_sceneObjectContainer, animation, layer, repeat);
        return AddObject(animationObject);
    }

    /// <inheritdoc />
    public ButtonObject AddButton(ButtonSceneElement button, int layer)
    {
        var buttonObject = new ButtonObject(_sceneObjectContainer, button, layer);
        return AddObject(buttonObject);
    }

    /// <inheritdoc />
    public ToggleButtonObject AddToggleButton(ToggleButtonSceneElement toggleButton, int layer)
    {
        var toggleButtonObject = new ToggleButtonObject(_sceneObjectContainer, toggleButton, layer);
        return AddObject(toggleButtonObject);
    }

    /// <inheritdoc />
    public TextListBoxObject AddTextListBox(TextListBoxSceneElement textListBox, int layer)
    {
        var textListBoxObject = new TextListBoxObject(this, _sceneObjectContainer, textListBox, layer);
        return AddObject(textListBoxObject);
    }

    /// <inheritdoc />
    public VideoGameObject AddVideo(Stream videoStream, RectangleD bounds, int layer, bool canSkip = true)
    {
        var videoObject = new VideoGameObject(_sceneObjectContainer, videoStream, bounds, layer, canSkip);
        return AddObject(videoObject);
    }

    /// <inheritdoc />
    public void UpdateGameObjects(long ticksCount)
    {
        for (var gameObjectNode = _gameObjects.First; gameObjectNode != null;)
        {
            var nextNode = gameObjectNode.Next;
            var gameObject = gameObjectNode.Value;

            if (gameObject.IsDestroyed)
            {
                _gameObjects.Remove(gameObjectNode);
            }
            else
            {
                gameObject.Update(ticksCount);
            }

            gameObjectNode = nextNode;
        }
    }

    /// <inheritdoc />
    public TGameObject AddObject<TGameObject>(TGameObject gameObject)
        where TGameObject : GameObject
    {
        gameObject.Initialize();
        _gameObjects.AddLast(gameObject);

        return gameObject;
    }
}