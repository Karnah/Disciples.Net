using System;
using Disciples.Common.Models;
using Disciples.Engine.Platform.Events;

namespace Disciples.Engine.Platform.Managers;

/// <summary>
/// Менеджер устройств ввода.
/// </summary>
public interface IInputManager
{
    /// <summary>
    /// Положение курсора мыши.
    /// </summary>
    PointD MousePosition { get; }


    /// <summary>
    /// Событие изменения состояния кнопки мыши.
    /// </summary>
    event EventHandler<MouseButtonEventArgs> MouseButtonEvent;

    /// <summary>
    /// Событие изменения состояния кнопки клавиатуры.
    /// </summary>
    event EventHandler<KeyButtonEventArgs> KeyButtonEvent;
}