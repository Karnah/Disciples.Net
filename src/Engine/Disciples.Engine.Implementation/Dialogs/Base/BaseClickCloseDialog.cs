using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Enums;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Dialogs.Base;

/// <summary>
/// Базовый класс для диалогов, которые закрываются по нажатию на кнопку.
/// </summary>
public abstract class BaseClickCloseDialog : BaseDialog
{
    private readonly IGameObjectContainer _gameObjectContainer;

    /// <summary>
    /// Создать объект типа <see cref="BaseClickCloseDialog" />.
    /// </summary>
    protected BaseClickCloseDialog(
        IGameObjectContainer gameObjectContainer,
        ISceneInterfaceController sceneInterfaceController,
        IInterfaceProvider interfaceProvider
        ) : base(gameObjectContainer, sceneInterfaceController, interfaceProvider)
    {
        _gameObjectContainer = gameObjectContainer;
    }

    /// <inheritdoc />
    protected override void OpenInternal(IReadOnlyList<GameObject> dialogGameObjects)
    {
        // Сразу снимаем выделение с объекта основной сцены, который был выделен.
        // Как правила это кнопка из-за которой открылся диалог.
        var beforeOpenSelectedGameObject = _gameObjectContainer
            .GameObjects
            .FirstOrDefault(go => go.SelectionComponent?.IsHover == true);
        beforeOpenSelectedGameObject?.SelectionComponent?.Unhovered();
    }

    /// <inheritdoc />
    public override void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        foreach (var inputDeviceEvent in inputDeviceEvents)
        {
            var inputDeviceGameObject = inputDeviceEvent.GameObject;

            // Обрабатываются только объекты диалога.
            if (inputDeviceEvent.ActionType != InputDeviceActionType.KeyboardButton &&
                !DialogGameObjects.Contains(inputDeviceGameObject))
            {
                continue;
            }

            // TODO Дублирование кода с базовой сценой.
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
                    foreach (var gameObject in DialogGameObjects.ToArray())
                    {
                        gameObject.MouseLeftButtonClickComponent?.PressedKeyboardButton(inputDeviceEvent.KeyboardButton!.Value);
                    }

                    break;
            }
        }
    }
}