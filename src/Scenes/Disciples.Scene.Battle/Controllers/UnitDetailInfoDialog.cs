using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Enums;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Диалог отображения информации о юните.
/// </summary>
internal class UnitDetailInfoDialog : BaseDialog
{
    private readonly IBattleGameObjectContainer _gameObjectContainer;
    private readonly Unit _unit;

    private DetailUnitInfoObject _detailUnitInfoObject = null!;
    private GameObject? _beforeOpenSelectedGameObject;
    private GameObject? _lastSelectedGameObject;

    /// <summary>
    /// Создать объект типа <see cref="UnitDetailInfoDialog" />.
    /// </summary>
    public UnitDetailInfoDialog(IBattleGameObjectContainer gameObjectContainer, Unit unit)
    {
        _gameObjectContainer = gameObjectContainer;
        _unit = unit;
    }

    /// <inheritdoc />
    public override void Open()
    {
        _beforeOpenSelectedGameObject = _gameObjectContainer
            .GameObjects
            .FirstOrDefault(go => go.SelectionComponent?.IsSelected == true);
        _detailUnitInfoObject = _gameObjectContainer.ShowDetailUnitInfo(_unit);
    }

    /// <inheritdoc />
    public override void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        // Запоминаем последний выбранный объект.
        var selectionEvent = inputDeviceEvents
            .LastOrDefault(e => e.ActionType == InputDeviceActionType.Selection);
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

        IsClosed = true;
        _detailUnitInfoObject.Destroy();

        // Обрабатываем событие изменения выбранного объекта.
        if (_beforeOpenSelectedGameObject != _lastSelectedGameObject)
        {
            _beforeOpenSelectedGameObject?.SelectionComponent!.Unselected();
            _lastSelectedGameObject?.SelectionComponent!.Selected();
        }

        // Прокидываем событие отжатой кнопки до объекта, на котором она была нажата.
        var pressedGameObject = _gameObjectContainer
            .GameObjects
            .FirstOrDefault(go => go.MouseRightButtonClickComponent?.IsPressed == true);
        pressedGameObject?.MouseRightButtonClickComponent!.Released();
    }
}