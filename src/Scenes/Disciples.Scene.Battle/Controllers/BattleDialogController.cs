using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc />
internal class BattleDialogController : IDialogController
{
    private readonly IDialogController _dialogController;
    private readonly IBattleGameObjectContainer _gameObjectContainer;

    /// <summary>
    /// Создать объект типа <see cref="BattleDialogController" />.
    /// </summary>
    public BattleDialogController(IDialogController dialogController, IBattleGameObjectContainer gameObjectContainer)
    {
        _dialogController = dialogController;
        _gameObjectContainer = gameObjectContainer;
    }

    /// <summary>
    /// Отобразить информацию об указанном юните.
    /// </summary>
    public void ShowUnitDetailInfo(Unit unit)
    {
        OpenDialog(new UnitDetailInfoDialog(_gameObjectContainer, unit));
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

    #endregion
}