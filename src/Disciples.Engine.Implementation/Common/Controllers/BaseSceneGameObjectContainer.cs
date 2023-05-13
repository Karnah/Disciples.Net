using System;
using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
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
    public AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true)
    {
        return GameObjectContainer.AddAnimation(frames, x, y, layer, repeat);
    }

    /// <inheritdoc />
    public ButtonObject AddButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer,
        KeyboardButton? hotkey = null)
    {
        return GameObjectContainer.AddButton(buttonStates, buttonPressedAction, x, y, layer, hotkey);
    }

    /// <inheritdoc />
    public ToggleButtonObject AddToggleButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer,
        KeyboardButton? hotkey = null)
    {
        return GameObjectContainer.AddToggleButton(buttonStates, buttonPressedAction, x, y, layer, hotkey);
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