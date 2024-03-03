using System;
using System.Collections.Generic;
using DryIoc;
using Microsoft.Extensions.Logging;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Dialogs;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Common.Controllers;

/// <inheritdoc />
internal class DialogController : IDialogController
{
    private readonly IResolver _resolver;
    private readonly ILogger<DialogController> _logger;

    private IDialog? _showingDialog;

    /// <summary>
    /// Создать объект типа <see cref="DialogController" />.
    /// </summary>
    public DialogController(IResolver resolver, ILogger<DialogController> logger)
    {
        _logger = logger;
        _resolver = resolver;
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
        _logger.LogInformation("Open dialog: {dialog}", dialog.GetType().Name);

        if (_showingDialog != null)
        {
            _logger.LogError("Impossible to open new dialog while the old one is not closed");
            return;
        }

        dialog.Open();
        _showingDialog = dialog;
    }

    /// <inheritdoc />
    public void ShowMessage(TextContainer message)
    {
        var messageDialog = _resolver.Resolve<MessageDialog>(new object[] { message });
        OpenDialog(messageDialog);
    }

    /// <inheritdoc />
    public void ShowConfirm(TextContainer message, Action onConfirm)
    {
        var confirmDialog = _resolver.Resolve<ConfirmDialog>(new object[] { message, onConfirm });
        OpenDialog(confirmDialog);
    }
}