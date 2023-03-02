using System;
using System.Collections.Generic;

using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Класс для "залипающей кнопки". То есть кнопка остаётся нажатой до тех пор, пока на неё не кликнут еще раз.
/// </summary>
public class ToggleButtonObject : ButtonObject
{
    private bool _isChecked;

    /// <summary>
    /// Создать объект типа <see cref="ToggleButtonObject" />.
    /// </summary>
    public ToggleButtonObject(
        ISceneObjectContainer sceneObjectContainer,
        IDictionary<SceneButtonState, IBitmap> buttonStates,
        Action buttonPressedAction,
        double x,
        double y,
        int layer,
        KeyboardButton? hotkey = null)
        : base(sceneObjectContainer, buttonStates, buttonPressedAction, x, y, layer, hotkey)
    {
        _isChecked = false;
    }

    /// <inheritdoc />
    public override void SetDisabled()
    {
        _isChecked = false;

        base.SetDisabled();
    }

    /// <inheritdoc />
    public override void SetSelected()
    {
        if (ButtonState == SceneButtonState.Pressed && _isChecked)
            return;

        base.SetSelected();
    }

    /// <inheritdoc />
    public override void SetUnselected()
    {
        if (ButtonState == SceneButtonState.Pressed && _isChecked)
            return;

        base.SetUnselected();
    }

    /// <inheritdoc />
    public override void Release()
    {
        // Отлавливаем ситуацию, когда кликнули, убрали мышь, вернули на место.
        if (ButtonState != SceneButtonState.Pressed)
            return;

        if (_isChecked)
            ButtonState = SceneButtonState.Selected;

        Click();
        UpdateButtonVisualObject();
    }

    /// <inheritdoc />
    public override void Click()
    {
        SetState(!_isChecked);

        base.Click();
    }

    /// <summary>
    /// Установить новое состояние кнопки.
    /// </summary>
    public void SetState(bool isChecked)
    {
        if (ButtonState == SceneButtonState.Disabled || _isChecked == isChecked)
            return;

        _isChecked = isChecked;
        ButtonState = _isChecked
            ? SceneButtonState.Pressed
            : SceneButtonState.Active;
        UpdateButtonVisualObject();
    }
}