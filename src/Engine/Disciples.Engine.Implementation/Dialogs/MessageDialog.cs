using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Dialogs.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Dialogs;

/// <summary>
/// Диалог с сообщением.
/// </summary>
public class MessageDialog : BaseClickCloseDialog
{
    private readonly TextContainer _message;

    /// <summary>
    /// Создать объект типа <see cref="MessageDialog" />.
    /// </summary>
    public MessageDialog(
        IGameObjectContainer gameObjectContainer,
        ISceneInterfaceController sceneInterfaceController,
        IInterfaceProvider interfaceProvider,
        TextContainer message)
        : base(gameObjectContainer, sceneInterfaceController, interfaceProvider)
    {
        _message = message;
    }

    /// <inheritdoc />
    protected override string DialogName => "DLG_MESSAGE_BOX";

    /// <inheritdoc />
    protected override void OpenInternal(IReadOnlyList<GameObject> dialogGameObjects)
    {
        base.OpenInternal(dialogGameObjects);

        dialogGameObjects.GetTextBlock("TXT_INFO", _message);
        dialogGameObjects.GetButton("BTN_OK", Close);
        dialogGameObjects.GetButton("BTN_YES", isHidden: true);
        dialogGameObjects.GetButton("BTN_NO", isHidden: true);
    }
}