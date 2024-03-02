using System;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Dialogs.Base;
using System.Collections.Generic;
using Disciples.Engine.Extensions;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Dialogs;

/// <summary>
/// Диалог подтверждения действия.
/// </summary>
internal class ConfirmDialog : BaseClickCloseDialog
{
    private readonly TextContainer _message;
    private readonly Action _onConfirm;

    /// <summary>
    /// Создать объект типа <see cref="ConfirmDialog" />.
    /// </summary>
    public ConfirmDialog(
        IGameObjectContainer gameObjectContainer,
        ISceneInterfaceController sceneInterfaceController,
        IInterfaceProvider interfaceProvider,
        TextContainer message,
        Action onConfirm
        ) : base(gameObjectContainer, sceneInterfaceController, interfaceProvider)
    {
        _message = message;
        _onConfirm = onConfirm;
    }

    /// <inheritdoc />
    protected override string DialogName => "DLG_MESSAGE_BOX";

    /// <inheritdoc />
    protected override void OpenInternal(IReadOnlyList<GameObject> dialogGameObjects)
    {
        base.OpenInternal(dialogGameObjects);

        dialogGameObjects.GetTextBlock("TXT_INFO", _message);
        dialogGameObjects.GetButton("BTN_OK", isHidden: true);
        dialogGameObjects.GetButton("BTN_YES", Confirm);
        dialogGameObjects.GetButton("BTN_NO", Close);
    }

    /// <summary>
    /// Обработать подтверждение игрока.
    /// </summary>
    private void Confirm()
    {
        _onConfirm.Invoke();
        Close();
    }
}