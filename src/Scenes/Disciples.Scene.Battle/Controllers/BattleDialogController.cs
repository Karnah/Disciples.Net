using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Dialogs;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc />
internal class BattleDialogController : IDialogController
{
    private readonly IDialogController _dialogController;
    private readonly IBattleGameObjectContainer _gameObjectContainer;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly ITextProvider _textProvider;
    private readonly ISceneInterfaceController _sceneInterfaceController;

    /// <summary>
    /// Создать объект типа <see cref="BattleDialogController" />.
    /// </summary>
    public BattleDialogController(
        IDialogController dialogController,
        IBattleGameObjectContainer gameObjectContainer,
        IBattleInterfaceProvider battleInterfaceProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        ITextProvider textProvider,
        ISceneInterfaceController sceneInterfaceController)
    {
        _dialogController = dialogController;
        _gameObjectContainer = gameObjectContainer;
        _battleInterfaceProvider = battleInterfaceProvider;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _textProvider = textProvider;
        _sceneInterfaceController = sceneInterfaceController;
    }

    /// <summary>
    /// Отобразить информацию об указанном юните.
    /// </summary>
    public void ShowUnitDetailInfo(Unit unit)
    {
        OpenDialog(new UnitDetailInfoDialog(_gameObjectContainer, _battleInterfaceProvider, _battleUnitResourceProvider, _textProvider, _sceneInterfaceController, unit));
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