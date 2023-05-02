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
        IsChecked = false;
    }

    /// <summary>
    /// Признак, что кнопка находится во включенном состоянии.
    /// </summary>
    public bool IsChecked { get; private set; }

    /// <inheritdoc />
    public override void SetDisabled()
    {
        IsChecked = false;

        base.SetDisabled();
    }

    /// <inheritdoc />
    public override void SetSelected()
    {
        if (ButtonState == SceneButtonState.Pressed && IsChecked)
            return;

        base.SetSelected();
    }

    /// <inheritdoc />
    public override void SetUnselected()
    {
        if (ButtonState == SceneButtonState.Pressed && IsChecked)
            return;

        base.SetUnselected();
    }

    /// <inheritdoc />
    public override void Release()
    {
        // Отлавливаем ситуацию, когда кликнули, убрали мышь, вернули на место.
        if (ButtonState != SceneButtonState.Pressed)
            return;

        if (IsChecked)
            ButtonState = SceneButtonState.Selected;

        Click();
        UpdateButtonVisualObject();
    }

    /// <inheritdoc />
    public override void Click()
    {
        SetState(!IsChecked);

        base.Click();
    }

    /// <summary>
    /// Установить новое состояние кнопки.
    /// </summary>
    public void SetState(bool isChecked)
    {
        if (ButtonState == SceneButtonState.Disabled || IsChecked == isChecked)
            return;

        IsChecked = isChecked;
        ButtonState = IsChecked
            ? SceneButtonState.Pressed
            : SceneButtonState.Active;
        UpdateButtonVisualObject();
    }
}