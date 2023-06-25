using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Models;
using Disciples.Engine.Enums;

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
    /// Объект, который будет использован для обработки событий ввода, если сейчас не выбран другой объект.
    /// </summary>
    protected virtual GameObject? DefaultInputGameObject => null;

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
        BeforeSceneUpdate(data);

        if (IsProcessInputDeviceEvents)
            ProcessInputDeviceEvents(data.InputDeviceEvents);

        GameObjectContainer.UpdateGameObjects(data.TicksCount);

        // При обработке событий от пользователя, может поменяться сцена.
        // В таком случае дальше обновлять не нужно.
        // TODO Вообще, это может стрельнуть где угодно. Нужно посмотреть, куда это лучше вынести.
        if (IsLoaded)
            AfterSceneUpdate();

        SceneObjectContainer.PlatformSceneObjectContainer.UpdateContainer();
    }

    /// <summary>
    /// Выполнить действия до обновления игровых объектов.
    /// </summary>
    protected abstract void BeforeSceneUpdate(UpdateSceneData data);

    /// <summary>
    /// Выполнить действия после игровых объектов.
    /// </summary>
    protected abstract void AfterSceneUpdate();

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
            var inputDeviceGameObject = inputDeviceEvent.GameObject ?? DefaultInputGameObject;

            switch (inputDeviceEvent.ActionType)
            {
                case InputDeviceActionType.Hover when inputDeviceEvent.ActionState == InputDeviceActionState.Activated:
                    inputDeviceGameObject?.SelectionComponent?.Hovered();
                    continue;
                case InputDeviceActionType.Hover when inputDeviceEvent.ActionState == InputDeviceActionState.Deactivated:
                    inputDeviceGameObject?.SelectionComponent?.Unhovered();
                    continue;

                case InputDeviceActionType.MouseLeft when inputDeviceEvent.ActionState == InputDeviceActionState.Activated:
                    inputDeviceGameObject?.MouseLeftButtonClickComponent?.Pressed();
                    continue;
                case InputDeviceActionType.MouseLeft when inputDeviceEvent.ActionState == InputDeviceActionState.Deactivated:
                    inputDeviceGameObject?.MouseLeftButtonClickComponent?.Clicked();
                    continue;

                case InputDeviceActionType.MouseRight when inputDeviceEvent.ActionState == InputDeviceActionState.Activated:
                    inputDeviceGameObject?.MouseRightButtonClickComponent?.Pressed();
                    continue;
                case InputDeviceActionType.MouseRight when inputDeviceEvent.ActionState == InputDeviceActionState.Deactivated:
                    inputDeviceGameObject?.MouseRightButtonClickComponent?.Released();
                    continue;

                case InputDeviceActionType.KeyboardButton when inputDeviceEvent.ActionState == InputDeviceActionState.Activated:
                    // TODO Оптимизация.
                    foreach (var gameObject in GameObjectContainer.GameObjects.ToArray())
                    {
                        gameObject.MouseLeftButtonClickComponent?.PressedKeyboardButton(inputDeviceEvent.KeyboardButton!.Value);
                    }
                    break;
            }
        }
    }
}