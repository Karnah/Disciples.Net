using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Enums;
using Disciples.Engine.Implementation.Extensions;
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
            if (inputDeviceEvent.ActionType == InputDeviceActionType.KeyboardButton)
                InputDeviceEventExtensions.ProcessKeyboardEvent(inputDeviceEvent.ActionState, inputDeviceEvent.KeyboardButton!.Value, DialogGameObjects);
            else
                InputDeviceEventExtensions.ProcessMouseEvent(inputDeviceEvent.ActionType, inputDeviceEvent.ActionState, inputDeviceEvent.GameObject);
        }
    }
}