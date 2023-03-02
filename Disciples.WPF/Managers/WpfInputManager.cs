using System;
using System.Windows;
using System.Windows.Input;

using Disciples.Engine.Common.Enums;
using Disciples.Engine.Platform.Enums;
using Disciples.Engine.Platform.Events;
using Disciples.Engine.Platform.Managers;

using MouseButton = Disciples.Engine.Platform.Enums.MouseButton;
using WpfMouseButton = System.Windows.Input.MouseButton;
using MouseButtonEventArgs = Disciples.Engine.Platform.Events.MouseButtonEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using Point = Disciples.Engine.Platform.Models.Point;

namespace Disciples.WPF.Managers;

/// <inheritdoc />
public class WpfInputManager : IInputManager
{
    /// <inheritdoc />
    public WpfInputManager()
    {
        EventManager.RegisterClassHandler(typeof(Window), UIElement.MouseDownEvent, new MouseButtonEventHandler(OnMouseUpDown));
        EventManager.RegisterClassHandler(typeof(Window), UIElement.MouseUpEvent, new MouseButtonEventHandler(OnMouseUpDown));
        EventManager.RegisterClassHandler(typeof(Window), UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        EventManager.RegisterClassHandler(typeof(Window), UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown));
    }

    /// <inheritdoc />
    public Point MousePosition { get; private set; }


    /// <inheritdoc />
    public event EventHandler<MouseButtonEventArgs> MouseButtonEvent;

    /// <inheritdoc />
    public event EventHandler<KeyButtonEventArgs> KeyButtonEvent;


    /// <summary>
    /// Обработать событие нажатия на кнопку мыши.
    /// </summary>
    private void OnMouseUpDown(object sender, WpfMouseButtonEventArgs args)
    {
        var gameWindow = sender as GameWindow;
        if (gameWindow == null)
            return;

        if (args.ChangedButton == WpfMouseButton.Left) {
            MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Left,
                args.ButtonState == MouseButtonState.Pressed
                    ? ButtonState.Pressed
                    : ButtonState.Released));

            return;
        }

        if (args.ChangedButton == WpfMouseButton.Right)
        {
            MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Right,
                args.ButtonState == MouseButtonState.Pressed
                    ? ButtonState.Pressed
                    : ButtonState.Released));

            return;
        }
    }

    /// <summary>
    /// Обработать событие изменения положения мыши.
    /// </summary>
    private void OnMouseMove(object sender, MouseEventArgs args)
    {
        var gameWindow = sender as GameWindow;
        if (gameWindow == null)
            return;

        var position = args.GetPosition(gameWindow.Field);
        MousePosition = new Point(position.X, position.Y);
    }

    /// <summary>
    /// Обработать событие нажатия на клавишу клавиатуры.
    /// </summary>
    private void OnKeyDown(object sender, KeyEventArgs args)
    {
        // Если были какие-то модификаторы, то игнорируем.
        if (args.KeyboardDevice.Modifiers != ModifierKeys.None)
            return;

        var keyboardButton = ToKeyboardButton(args.Key);
        if (keyboardButton == null)
            return;

        // todo Не обрабатываю событие того, что кнопку отпустили. Вроде как сейчас и не нужно.
        KeyButtonEvent?.Invoke(this, new KeyButtonEventArgs(keyboardButton.Value, ButtonState.Pressed));
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