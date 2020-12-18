using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Platform.Enums;
using Disciples.Engine.Platform.Events;

using IInputManager = Disciples.Engine.Platform.Managers.IInputManager;
using MouseButton = Disciples.Engine.Platform.Enums.MouseButton;
using Point = Disciples.Engine.Platform.Models.Point;

namespace Disciples.Avalonia.Managers
{
    /// <inheritdoc />
    public class AvaloniaInputManager : IInputManager
    {
        private Point? _screenOffset;

        /// <inheritdoc />
        public AvaloniaInputManager()
        {
            Application.Current.InputManager.PostProcess.OfType<RawMouseEventArgs>().Subscribe(OnMouseStateChanged);
            Application.Current.InputManager.PostProcess.OfType<RawKeyEventArgs>().Subscribe(OnKeyStateChanged);
        }


        /// <inheritdoc />
        public Point MousePosition { get; private set; }


        /// <inheritdoc />
        public event EventHandler<MouseButtonEventArgs> MouseButtonEvent;

        /// <inheritdoc />
        public event EventHandler<KeyButtonEventArgs> KeyButtonEvent;


        /// <summary>
        /// Обработать событие от курсора.
        /// </summary>
        private void OnMouseStateChanged(RawMouseEventArgs args)
        {
            switch (args.Type)
            {
                case RawMouseEventType.Move:
                    // Один раз рассчитываем смещение игрового поля относительно левого края экрана.
                    // Предполагаем, что поле выровнено по центру экрана.
                    if (_screenOffset == null)
                    {
                        var window = (Window)args.Root;
                        _screenOffset = new Point((window.Width - GameInfo.Width) / 2, (window.Height - GameInfo.Height) / 2);
                    }

                    MousePosition = new Point((int)(args.Position.X - _screenOffset?.X) / GameInfo.Scale, (int)(args.Position.Y - _screenOffset?.Y) / GameInfo.Scale);
                    break;

                case RawMouseEventType.LeftButtonDown:
                    MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Left, ButtonState.Pressed));
                    break;

                case RawMouseEventType.LeftButtonUp:
                    MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Left, ButtonState.Released));
                    break;

                case RawMouseEventType.RightButtonDown:
                    MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Right, ButtonState.Pressed));
                    break;

                case RawMouseEventType.RightButtonUp:
                    MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Right, ButtonState.Released));
                    break;
            }
        }

        /// <summary>
        /// Обработать событие от клавиатуры.
        /// </summary>
        private void OnKeyStateChanged(RawKeyEventArgs args)
        {
            // Если были какие-то модификаторы, то игнорируем.
            if (args.Modifiers != InputModifiers.None)
                return;

            var keyboardButton = ToKeyboardButton(args.Key);
            if (keyboardButton == null)
                return;

            KeyButtonEvent?.Invoke(this, new KeyButtonEventArgs(
                keyboardButton.Value,
                args.Type == RawKeyEventType.KeyDown
                    ? ButtonState.Pressed
                    : ButtonState.Released));
        }

        /// <summary>
        /// Получить нажатую клавишу.
        /// </summary>
        private static KeyboardButton? ToKeyboardButton(Key key)
        {
            switch (key)
            {
                case Key.Tab:
                    return KeyboardButton.Tab;
                case Key.A:
                    return KeyboardButton.A;
                case Key.D:
                    return KeyboardButton.D;
                case Key.I:
                    return KeyboardButton.I;
                case Key.P:
                    return KeyboardButton.P;
                case Key.R:
                    return KeyboardButton.R;
                case Key.S:
                    return KeyboardButton.S;
                case Key.W:
                    return KeyboardButton.W;
                default:
                    return null;
            }
        }
    }
}