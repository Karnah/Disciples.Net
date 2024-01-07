using System;
using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <inheritdoc />
internal class DialogController : IDialogController
{
    private readonly ILogger _logger;

    private IDialog? _showingDialog;

    /// <summary>
    /// Создать объект типа <see cref="DialogController" />.
    /// </summary>
    public DialogController(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsDialogShowing => _showingDialog != null;

    /// <inheritdoc />
    public void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        if (_showingDialog == null)
            return;

        _showingDialog.ProcessInputDeviceEvents(inputDeviceEvents);

        if (_showingDialog.IsClosed)
            _showingDialog = null;
    }

    /// <inheritdoc />
    public void OpenDialog(IDialog dialog)
    {
        if (_showingDialog != null)
        {
            _logger.LogError("Невозможно открыть новый диалог, пока отображается старый", new ArgumentException());
            return;
        }

        dialog.Open();
        _showingDialog = dialog;
    }
}