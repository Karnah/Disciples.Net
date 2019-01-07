using System;
using Disciples.Engine.Platform.Enums;

namespace Disciples.Engine.Platform.Events
{
    /// <summary>
    /// Аргументы события изменения состояния кнопок мыши.
    /// </summary>
    public class MouseButtonEventArgs : EventArgs
    {
        /// <inheritdoc />
        public MouseButtonEventArgs(MouseButton mouseButton, ButtonState buttonState)
        {
            MouseButton = mouseButton;
            ButtonState = buttonState;
        }


        /// <summary>
        /// Кнопка, которая изменила состояние.
        /// </summary>
        public MouseButton MouseButton { get; }

        /// <summary>
        /// Нажата или отпущена была кнопка.
        /// </summary>
        public ButtonState ButtonState { get; }
    }
}