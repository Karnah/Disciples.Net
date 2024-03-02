using DryIoc;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Dialogs;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc />
internal class BattleDialogController : IDialogController
{
    private readonly IResolver _resolver;
    private readonly IDialogController _dialogController;

    /// <summary>
    /// Создать объект типа <see cref="BattleDialogController" />.
    /// </summary>
    public BattleDialogController(
        IResolver resolver,
        IDialogController dialogController)
    {
        _dialogController = dialogController;
        _resolver = resolver;
    }

    /// <summary>
    /// Отобразить информацию об указанном юните.
    /// </summary>
    public void ShowUnitDetailInfo(Unit unit)
    {
        var unitDetailInfoDialog = _resolver.Resolve<UnitDetailInfoDialog>(new object[] { unit });
        OpenDialog(unitDetailInfoDialog);
    }

    #region IDialogController

    /// <inheritdoc />
    public bool IsDialogShowing => _dialogController.IsDialogShowing;

    /// <inheritdoc />
    public void ProcessInputDeviceEvents(IReadOnlyList<InputDeviceEvent> inputDeviceEvents)
    {
        _dialogController.ProcessInputDeviceEvents(inputDeviceEvents);
    }

    /// <inheritdoc />
    public void OpenDialog(IDialog dialog)
    {
        _dialogController.OpenDialog(dialog);
    }

    /// <inheritdoc />
    public void ShowMessage(TextContainer message)
    {
        _dialogController.ShowMessage(message);
    }

    /// <inheritdoc />
    public void ShowConfirm(TextContainer message, Action onConfirm)
    {
        _dialogController.ShowConfirm(message, onConfirm);
    }

    #endregion
}