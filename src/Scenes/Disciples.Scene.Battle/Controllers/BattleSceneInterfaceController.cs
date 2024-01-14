using Disciples.Common.Models;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc />
internal class BattleSceneInterfaceController : SceneInterfaceController
{
    private readonly BattleContext _battleContext;

    /// <summary>
    /// Создать объект типа <see cref="BattleSceneInterfaceController" />.
    /// </summary>
    public BattleSceneInterfaceController(
        IBattleGameObjectContainer gameObjectContainer,
        BattleContext battleContext
        ) : base(gameObjectContainer)
    {
        _battleContext = battleContext;
    }

    /// <inheritdoc />
    protected override GameObject? GetElement(SceneElement sceneElement, Layers layers)
    {
        // Эти элементы выступают в роли плейсхолдеров. Для них нет необходимости создавать объекты.
        if (sceneElement.Name.StartsWith(BattleUnitPanelElementNames.LEFT_PANEL_DISPLAY_PLACEHOLDER) ||
            sceneElement.Name.StartsWith(BattleUnitPanelElementNames.LEFT_UNIT_PANEL_PORTRAIT_PATTERN_PLACEHOLDER) ||
            sceneElement.Name.StartsWith(BattleUnitPanelElementNames.LEFT_UNIT_PANEL_HP_PATTERN_PLACEHOLDER) ||
            sceneElement.Name.StartsWith(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_PORTRAIT_PATTERN_PLACEHOLDER) ||
            sceneElement.Name.StartsWith(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_HP_PATTERN_PLACEHOLDER) ||
            sceneElement.Name.StartsWith(BattlegroundElementNames.ATTACK_UNIT_PATTERN_PLACEHOLDER) ||
            sceneElement.Name.StartsWith(BattlegroundElementNames.DEFEND_UNIT_PATTERN_PLACEHOLDER) ||
            sceneElement.Name
                is BattleBottomPanelElementNames.LEFT_UNIT_PORTRAIT_IMAGE
                or BattleBottomPanelElementNames.LEFT_LEADER_ITEMS_IMAGE
                or BattleBottomPanelElementNames.LEFT_UNIT_INFO_TEXT_BLOCK ||
            sceneElement.Name
                is BattleBottomPanelElementNames.RIGHT_UNIT_PORTRAIT_IMAGE
                or BattleBottomPanelElementNames.RIGHT_LEADER_ITEMS_IMAGE
                or BattleBottomPanelElementNames.RIGHT_UNIT_INFO_TEXT_BLOCK)
        {
            return null;
        }

        // Если ИИ напал на игрока, то кнопка переключения панели должна быть слева.
        if (sceneElement.Name == BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_REFLECT_TOGGLE_BUTTON &&
            _battleContext.PlayerSquadPosition == BattleSquadPosition.Defender)
        {
            var reflectToggleButton = (ToggleButtonSceneElement)sceneElement;
            var position = new RectangleD(
                138,
                reflectToggleButton.Position.Y - 2,
                reflectToggleButton.Position.Width,
                reflectToggleButton.Position.Height);
            return base.GetElement(new ToggleButtonSceneElement
            {
                Name = reflectToggleButton.Name,
                Position = position,
                ButtonStates = reflectToggleButton.ButtonStates,
                CheckedButtonStates = reflectToggleButton.CheckedButtonStates,
                ToolTip = reflectToggleButton.ToolTip,
                HotKeys = reflectToggleButton.HotKeys
            }, layers);
        }

        return base.GetElement(sceneElement, layers);
    }
}