using System.Collections.Generic;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Controllers;

/// <summary>
/// Диалог.
/// </summary>
public interface IDialog
{
    /// <summary>
    /// Признак, что диалог закрыт.
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Открыть диалог.
    /// </summary>
    void Open();

    /// <summary>
    /// Обработать события ввода пользователя.
    /// </summary>
    void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents);
}