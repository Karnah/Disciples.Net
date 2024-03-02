using System;
using System.Collections.Generic;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Controllers;

/// <summary>
/// Контроллер для дополнительных сообщений пользователю,
/// Которые выводятся поверх основной сцены и блокирует её интерфейс.
/// </summary>
public interface IDialogController
{
    /// <summary>
    /// Признак, что в данный момент отображается диалог.
    /// </summary>
    bool IsDialogShowing { get; }

    /// <summary>
    /// Обработать события ввода пользователя.
    /// </summary>
    void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents);

    /// <summary>
    /// Открыть диалог.
    /// </summary>
    void OpenDialog(IDialog dialog);

    /// <summary>
    /// Отобразить сообщение.
    /// </summary>
    void ShowMessage(TextContainer message);

    /// <summary>
    /// Отобразить диалог с предложением подтвердить действие.
    /// </summary>
    void ShowConfirm(TextContainer message, Action onConfirm);
}