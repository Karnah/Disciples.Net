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
        IReadOnlyDictionary<SceneButtonState, IBitmap> buttonStates,
        Action buttonPressedAction,
        double x,
        double y,
        int layer,
        IReadOnlyList<KeyboardButton> hotKeys)
        : base(sceneObjectContainer, buttonStates, buttonPressedAction, x, y, layer, hotKeys)
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
    protected override void OnSelected()
    {
        // Если IsChecked=true, то кнопка находится в состоянии Pressed.
        // В этом статусе выделение не применяется.
        if (IsChecked)
            return;

        base.OnSelected();
    }

    /// <inheritdoc />
    protected override void OnUnselected()
    {
        // Если IsChecked=true, то кнопка находится в состоянии Pressed.
        // В этом статусе выделение не применяется.
        if (IsChecked)
            return;

        base.OnUnselected();
    }

    /// <inheritdoc />
    protected override SceneButtonState ProcessClickInternal()
    {
        IsChecked = !IsChecked;

        var buttonState = base.ProcessClickInternal();
        return IsChecked
            ? SceneButtonState.Pressed
            : buttonState;
    }

    /// <summary>
    /// Установить новое состояние кнопки.
    /// </summary>
    public void SetState(bool isChecked)
    {
        if (ButtonState == SceneButtonState.Disabled || IsChecked == isChecked)
            return;

        IsChecked = isChecked;
        SetState(IsChecked
            ? SceneButtonState.Pressed
            : SceneButtonState.Active);
    }
}