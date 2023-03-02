using System;

using Disciples.Engine.Common.Enums;
using Disciples.Engine.Platform.Enums;

namespace Disciples.Engine.Platform.Events;

/// <summary>
/// Аргументы события изменения состояния кнопки клавиатуры.
/// </summary>
public class KeyButtonEventArgs : EventArgs
{
    /// <inheritdoc />
    public KeyButtonEventArgs(KeyboardButton keyboardButton, ButtonState buttonState)
    {
        KeyboardButton = keyboardButton;
        ButtonState = buttonState;
    }


    /// <summary>
    /// Кнопка, которая изменила свой состояние.
    /// </summary>
    public KeyboardButton KeyboardButton { get; }

    /// <summary>
    /// Нажата или отпущена была кнопка.
    /// </summary>
    public ButtonState ButtonState { get; }
}