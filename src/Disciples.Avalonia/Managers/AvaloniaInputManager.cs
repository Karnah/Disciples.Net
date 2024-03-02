using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Disciples.Avalonia.Models;
using Disciples.Common.Models;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Platform.Enums;
using Disciples.Engine.Platform.Events;

using IInputManager = Disciples.Engine.Platform.Managers.IInputManager;
using MouseButton = Disciples.Engine.Platform.Enums.MouseButton;

namespace Disciples.Avalonia.Managers;

/// <inheritdoc />
public class AvaloniaInputManager : IInputManager
{
    private readonly AvaloniaGameInfo _gameInfo;

    /// <summary>
    /// Создать объект типа <see cref="AvaloniaInputManager" />.
    /// </summary>
    public AvaloniaInputManager(AvaloniaGameInfo gameInfo)
    {
        _gameInfo = gameInfo;

        InputElement.PointerPressedEvent.AddClassHandler(typeof(Window), OnMouseUpDown);
        InputElement.PointerReleasedEvent.AddClassHandler(typeof(Window), OnMouseUpDown);
        InputElement.PointerMovedEvent.AddClassHandler(typeof(Window), OnMouseMove);
        InputElement.PointerWheelChangedEvent.AddClassHandler(typeof(Window), OnMouseWheelMove);
        InputElement.KeyDownEvent.AddClassHandler(typeof(Window), OnKeyDown);
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
    private void OnMouseUpDown(object? sender, RoutedEventArgs args)
    {
        var pointerEventArgs = args as PointerEventArgs;
        if (pointerEventArgs == null)
            return;

        var pointerUpdateKind = pointerEventArgs.GetCurrentPoint(null).Properties.PointerUpdateKind;
        switch (pointerUpdateKind)
        {
            case PointerUpdateKind.LeftButtonPressed:
                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Left, ButtonState.Pressed));
                break;
            case PointerUpdateKind.RightButtonPressed:
                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Right, ButtonState.Pressed));
                break;
            case PointerUpdateKind.LeftButtonReleased:
                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Left, ButtonState.Released));
                break;
            case PointerUpdateKind.RightButtonReleased:
                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(MouseButton.Right, ButtonState.Released));
                break;
        }
    }

    /// <summary>
    /// Обработать событие изменения положения мыши.
    /// </summary>
    private void OnMouseMove(object? sender, RoutedEventArgs args)
    {
        var pointerEventArgs = args as PointerEventArgs;
        if (pointerEventArgs == null)
            return;

        var position = pointerEventArgs.GetPosition(null) * _gameInfo.FieldTransform;
        MousePosition = new PointD(position.X, position.Y);
    }

    /// <summary>
    /// Обработать событие изменения колёсика мыши.
    /// </summary>
    private void OnMouseWheelMove(object? sender, RoutedEventArgs args)
    {
        var pointerWheelEventArgs = args as PointerWheelEventArgs;
        if (pointerWheelEventArgs == null)
            return;

        var keyboardButton = pointerWheelEventArgs.Delta.Y > 0
            ? KeyboardButton.Up
            : KeyboardButton.Down;
        KeyButtonEvent?.Invoke(this, new KeyButtonEventArgs(keyboardButton, ButtonState.Pressed));
    }

    /// <summary>
    /// Обработать событие нажатия на клавишу клавиатуры.
    /// </summary>
    private void OnKeyDown(object? sender, RoutedEventArgs args)
    {
        var keyEventArgs = (KeyEventArgs)args;

        // Если были какие-то модификаторы, то игнорируем.
        if (keyEventArgs.KeyModifiers != KeyModifiers.None)
            return;

        var keyboardButton = ToKeyboardButton(keyEventArgs.Key);
        if (keyboardButton == null)
            return;

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