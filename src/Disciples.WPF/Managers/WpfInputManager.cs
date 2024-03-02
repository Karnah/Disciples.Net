using System;
using System.Windows;
using System.Windows.Input;
using Disciples.Common.Models;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Platform.Enums;
using Disciples.Engine.Platform.Events;
using Disciples.Engine.Platform.Managers;
using Disciples.WPF.Models;
using MouseButton = Disciples.Engine.Platform.Enums.MouseButton;
using WpfMouseButton = System.Windows.Input.MouseButton;
using MouseButtonEventArgs = Disciples.Engine.Platform.Events.MouseButtonEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;

namespace Disciples.WPF.Managers;

/// <inheritdoc />
public class WpfInputManager : IInputManager
{
    private readonly WpfGameInfo _gameInfo;

    /// <summary>
    /// Создать объект типа <see cref="WpfInputManager" />.
    /// </summary>
    public WpfInputManager(WpfGameInfo gameInfo)
    {
        _gameInfo = gameInfo;

        EventManager.RegisterClassHandler(typeof(Window), UIElement.MouseDownEvent, new MouseButtonEventHandler(OnMouseUpDown));
        EventManager.RegisterClassHandler(typeof(Window), UIElement.MouseUpEvent, new MouseButtonEventHandler(OnMouseUpDown));
        EventManager.RegisterClassHandler(typeof(Window), UIElement.MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        EventManager.RegisterClassHandler(typeof(Window), UIElement.MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheelMove));
        EventManager.RegisterClassHandler(typeof(Window), UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown));
    }

    /// <inheritdoc />
    public PointD MousePosition { get; private set; }

    /// <inheritdoc />
    public event EventHandler<MouseButtonEventArgs>? MouseButtonEvent;

    /// <inheritdoc />
    public event EventHandler<KeyButtonEventArgs>? KeyButtonEvent;

    /// <summary>
    /// Обработать событие нажатия на кнопку мыши.
    /// </summary>
    private void OnMouseUpDown(object? sender, WpfMouseButtonEventArgs args)
    {
        if (args.ChangedButton == WpfMouseButton.Left)
        {
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
    private void OnMouseMove(object? sender, MouseEventArgs args)
    {
        // TODO GetPosition оооочень медленный метод. Нужно найти способ оптимизировать вызов.
        // Вызывать расчет только при вызове MousePosition недостаточно быстрый способ.
        var position = args.GetPosition(_gameInfo.GameFiled);
        MousePosition = new PointD(position.X, position.Y);
    }

    /// <summary>
    /// Обработать событие изменения колёсика мыши.
    /// </summary>
    private void OnMouseWheelMove(object? sender, MouseWheelEventArgs args)
    {
        var keyboardButton = args.Delta > 0
            ? KeyboardButton.Up
            : KeyboardButton.Down;
        KeyButtonEvent?.Invoke(this, new KeyButtonEventArgs(keyboardButton, ButtonState.Pressed));
    }

    /// <summary>
    /// Обработать событие нажатия на клавишу клавиатуры.
    /// </summary>
    private void OnKeyDown(object? sender, KeyEventArgs args)
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
        return key switch
        {
            Key.Tab => KeyboardButton.Tab,
            Key.Enter => KeyboardButton.Enter,
            Key.Escape => KeyboardButton.Escape,
            Key.PageUp => KeyboardButton.PageUp,
            Key.PageDown => KeyboardButton.PageDown,
            Key.Up => KeyboardButton.Up,
            Key.Down => KeyboardButton.Down,
            Key.A => KeyboardButton.A,
            Key.D => KeyboardButton.D,
            Key.I => KeyboardButton.I,
            Key.P => KeyboardButton.P,
            Key.R => KeyboardButton.R,
            Key.S => KeyboardButton.S,
            Key.W => KeyboardButton.W,
            _ => null
        };
    }
}