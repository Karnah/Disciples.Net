using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Models;
using Disciples.Engine.Enums;
using Disciples.Engine.Implementation.Extensions;

namespace Disciples.Engine.Implementation.Base;

/// <summary>
/// Базовый класс для всех сцен.
/// </summary>
public abstract class BaseScene : BaseSupportLoading, IScene
{
    private readonly IDialogController _dialogController;

    /// <summary>
    /// Создать объект типа <see cref="BaseScene" />.
    /// </summary>
    protected BaseScene(IGameObjectContainer gameObjectContainer, ISceneObjectContainer sceneObjectContainer, IDialogController dialogController)
    {
        GameObjectContainer = gameObjectContainer;
        SceneObjectContainer = sceneObjectContainer;
        _dialogController = dialogController;
    }

    /// <inheritdoc />
    public IGameObjectContainer GameObjectContainer { get; }

    /// <inheritdoc />
    public ISceneObjectContainer SceneObjectContainer { get; }

    /// <inheritdoc />
    public virtual CursorType DefaultCursorType => CursorType.Default;

    /// <summary>
    /// Объект, который будет использован для обработки всех событий ввода.
    /// <see langwrod="null" />, если нужно обрабатывать все объекты сцены.
    /// </summary>
    protected virtual GameObject? MainInputGameObject => null;

    /// <summary>
    /// Признак, что базовый класс должен сам обрабатывать события ввода пользователя.
    /// </summary>
    protected virtual bool IsProcessInputDeviceEvents => true;

    /// <inheritdoc />
    public void AfterSceneLoaded()
    {
        AfterSceneLoadedInternal();
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
    }

    /// <summary>
    /// Обработать загрузку сцены.
    /// </summary>
    protected virtual void AfterSceneLoadedInternal()
    {

    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <inheritdoc />
    public void UpdateScene(UpdateSceneData data)
    {
        // IsLoaded == false, признак, что сцена уже завершена и сейчас происходит переход на новую сцену.
        // Требуется обновлять игровые объекты и только.
        if (IsLoaded)
        {
            BeforeSceneUpdate(data);

            if (IsProcessInputDeviceEvents)
                ProcessInputDeviceEvents(data.InputDeviceEvents);
        }

        GameObjectContainer.UpdateGameObjects(data.TicksCount);

        // Здесь сцена либо уже была завершена заранее, либо завершилась при обработке событий ввода.
        if (IsLoaded)
            AfterSceneUpdate();

        SceneObjectContainer.PlatformSceneObjectContainer.UpdateContainer();
    }

    /// <summary>
    /// Выполнить действия до обновления игровых объектов.
    /// </summary>
    protected virtual void BeforeSceneUpdate(UpdateSceneData data)
    {

    }

    /// <summary>
    /// Выполнить действия после обновления игровых объектов.
    /// </summary>
    protected virtual void AfterSceneUpdate()
    {

    }

    /// <summary>
    /// Обработать события ввода пользователя.
    /// </summary>
    protected virtual void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        // Если открыт диалог, то события обрабатывает он.
        if (_dialogController.IsDialogShowing)
        {
            _dialogController.ProcessInputDeviceEvents(inputDeviceEvents);
            return;
        }

        foreach (var inputDeviceEvent in inputDeviceEvents)
        {
            if (inputDeviceEvent.ActionType == InputDeviceActionType.KeyboardButton)
            {
                var gameObjects = MainInputGameObject == null
                    ? GameObjectContainer.GameObjects
                    : new[] { MainInputGameObject };
                InputDeviceEventExtensions.ProcessKeyboardEvent(inputDeviceEvent.ActionState, inputDeviceEvent.KeyboardButton!.Value, gameObjects);
            }
            else
            {
                var gameObject = MainInputGameObject ?? inputDeviceEvent.GameObject;
                InputDeviceEventExtensions.ProcessMouseEvent(inputDeviceEvent.ActionType, inputDeviceEvent.ActionState, gameObject);
            }
        }
    }
}