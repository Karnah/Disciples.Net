using System;
using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
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
    public AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true)
    {
        var animation = new AnimationObject(_sceneObjectContainer, frames, x, y, layer, repeat);
        return AddObject(animation);
    }

    /// <inheritdoc />
    public ButtonObject AddButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null)
    {
        var button = new ButtonObject(_sceneObjectContainer, buttonStates, buttonPressedAction, x, y, layer, hotkey);
        return AddObject(button);
    }

    /// <inheritdoc />
    public ToggleButtonObject AddToggleButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null)
    {
        var toggleButton = new ToggleButtonObject(_sceneObjectContainer, buttonStates, buttonPressedAction, x, y, layer, hotkey);
        return AddObject(toggleButton);
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