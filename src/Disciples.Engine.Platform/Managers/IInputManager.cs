using System;

using Disciples.Engine.Platform.Events;
using Disciples.Engine.Platform.Models;

namespace Disciples.Engine.Platform.Managers;

/// <summary>
/// Менеджер устройств ввода.
/// </summary>
public interface IInputManager
{
    /// <summary>
    /// Положение курсора мыши.
    /// </summary>
    Point MousePosition { get; }


    /// <summary>
    /// Событие изменения состояния кнопки мыши.
    /// </summary>
    event EventHandler<MouseButtonEventArgs> MouseButtonEvent;

    /// <summary>
    /// Событие изменения состояния кнопки клавиатуры.
    /// </summary>
    event EventHandler<KeyButtonEventArgs> KeyButtonEvent;
}