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
/// Базовый класс для диалогов, которые остаются открытыми до тех пор, пока игрок держит ПКМ зажатой.
/// </summary>
public abstract class BaseReleaseButtonCloseDialog : BaseDialog
{
    private readonly IGameObjectContainer _gameObjectContainer;

    /// <summary>
    /// Особенность таких диалогов - выделение не снимается с объекта, пока открыт диалог.
    /// После закрытия диалога, нужно выделить новый объект, если игрок перемещал курсор.
    /// Для этого запоминаем объект, который был выделен до открытия диалога,
    /// И объект, который был выделен уже после его открытия.
    /// </summary>
    private GameObject? _beforeOpenSelectedGameObject;
    private GameObject? _lastSelectedGameObject;

    /// <inheritdoc />
    protected BaseReleaseButtonCloseDialog(
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
        _beforeOpenSelectedGameObject = _gameObjectContainer
            .GameObjects
            .FirstOrDefault(go => go.SelectionComponent?.IsHover == true);
        _lastSelectedGameObject = _beforeOpenSelectedGameObject;
    }

    /// <inheritdoc />
    public override void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        // Запоминаем последний выбранный объект.
        var selectionEvent = inputDeviceEvents
            .LastOrDefault(e => e.ActionType == InputDeviceActionType.Hover);
        if (selectionEvent != null)
        {
            _lastSelectedGameObject = selectionEvent.ActionState == InputDeviceActionState.Activated
                ? selectionEvent.GameObject
                : null;
        }

        // Диалог закрывается по отжатой ПКМ.
        var releasedRightMouseButtonEvent = inputDeviceEvents
            .FirstOrDefault(e => e.ActionType == InputDeviceActionType.MouseRight && e.ActionState == InputDeviceActionState.Deactivated);
        if (releasedRightMouseButtonEvent == null)
            return;

        Close();
    }

    /// <inheritdoc />
    protected override void Close()
    {
        base.Close();

        // Обрабатываем событие изменения выбранного объекта.
        if (_beforeOpenSelectedGameObject != _lastSelectedGameObject)
        {
            _beforeOpenSelectedGameObject?.SelectionComponent!.Unhovered();
            _lastSelectedGameObject?.SelectionComponent!.Hovered();
        }

        // Прокидываем событие отжатой кнопки до объекта, на котором она была нажата.
        var mouseRightButtonPressedGameObject = _gameObjectContainer
            .GameObjects
            .FirstOrDefault(go => go.MouseRightButtonClickComponent?.IsPressed == true);
        mouseRightButtonPressedGameObject?.MouseRightButtonClickComponent!.Released();
    }
}