using System.Collections.Generic;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <summary>
/// Базовый класс для диалога.
/// </summary>
public abstract class BaseDialog : IDialog
{
    /// <inheritdoc />
    public bool IsClosed { get; protected set; }

    /// <inheritdoc />
    public abstract void Open();

    /// <inheritdoc />
    public abstract void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents);
}