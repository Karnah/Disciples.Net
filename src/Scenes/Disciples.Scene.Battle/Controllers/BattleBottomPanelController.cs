using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контроллер для управления нижней панелью.
/// </summary>
internal class BattleBottomPanelController : BaseSupportLoading
{
    private readonly IBattleGameObjectContainer _gameObjectContainer;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly IBattleInterfaceProvider _interfaceProvider;
    private readonly BattleUnitActionController _unitActionController;
    private readonly BattleContext _context;
    private readonly IGameController _gameController;

    private ButtonObject? _defendButton;
    private ButtonObject? _retreatButton;
    private ButtonObject? _waitButton;
    private ButtonObject? _instantResolveButton;
    private ToggleButtonObject? _autoBattleButton;

    private ButtonObject? _openSquadInventoryButton;
    private ButtonObject? _exitButton;

    /// <summary>
    /// Создать объект типа <see cref="BattleBottomPanelController" />.
    /// </summary>
    public BattleBottomPanelController(
        IBattleGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IBattleInterfaceProvider interfaceProvider,
        BattleUnitActionController unitActionController,
        BattleContext context,
        IGameController gameController)
    {
        _gameObjectContainer = gameObjectContainer;
        _sceneObjectContainer = sceneObjectContainer;
        _interfaceProvider = interfaceProvider;
        _unitActionController = unitActionController;
        _context = context;
        _gameController = gameController;
    }

    /// <summary>
    /// Обработать начало действий на сцене.
    /// </summary>
    public void ProcessActionsBegin()
    {
        DisableButtons(_defendButton, _retreatButton, _waitButton);
    }

    /// <summary>
    /// Обработать завершение всех действий на сцене.
    /// </summary>
    public void ProcessActionsCompleted()
    {
        // Если юнит наносит второй удар, то указанные кнопки активировать не нужно.
        if (!_context.IsSecondAttack)
        {
            ActivateButtons(_defendButton, _retreatButton);

            // Если юнит уже ждал на этом ходу, то больше ждать не может.
            if (!_context.IsWaitingUnitTurn)
                ActivateButtons(_waitButton);
        }
    }

    /// <summary>
    /// Обработать завершение битвы.
    /// </summary>
    public void ProcessBattleCompleted()
    {
        RemoveButton(ref _defendButton);
        RemoveButton(ref _retreatButton);
        RemoveButton(ref _waitButton);
        RemoveButton(ref _instantResolveButton);
        RemoveButton(ref _autoBattleButton);

        // Возможность открыть инвентарь будет только, если победил игрок при атаке.
        if (_context.BattleWinnerSquad == BattleSquadPosition.Attacker && !_context.AttackingSquad.Player.IsComputer)
        {
            _openSquadInventoryButton = _gameObjectContainer.AddButton(_interfaceProvider.OpenSquadInventoryButton, ExecuteOpenSquadInventory, 343, 524, BattleLayers.INTERFACE_LAYER, KeyboardButton.P);
            _openSquadInventoryButton.SetActive();
        }

        _exitButton = _gameObjectContainer.AddButton(_interfaceProvider.ExitButton, ExecuteExit, 419, 524, BattleLayers.INTERFACE_LAYER, KeyboardButton.Escape);
        _exitButton.SetActive();
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _sceneObjectContainer.AddImage(_interfaceProvider.BottomPanel, 0, GameInfo.OriginalHeight - _interfaceProvider.BottomPanel.Height, BattleLayers.PANEL_LAYER);

        _defendButton = _gameObjectContainer.AddButton(_interfaceProvider.DefendButton, ExecuteDefend, 380, 504, BattleLayers.INTERFACE_LAYER, KeyboardButton.D);
        _retreatButton = _gameObjectContainer.AddButton(_interfaceProvider.RetreatButton, ExecuteRetreat, 343, 524, BattleLayers.INTERFACE_LAYER, KeyboardButton.R);
        _waitButton = _gameObjectContainer.AddButton(_interfaceProvider.WaitButton, ExecuteWait, 419, 524, BattleLayers.INTERFACE_LAYER, KeyboardButton.W);
        _instantResolveButton = _gameObjectContainer.AddButton(_interfaceProvider.InstantResolveButton, ExecuteInstantBattle, 359, 563, BattleLayers.INTERFACE_LAYER, new [] { KeyboardButton.I, KeyboardButton.Escape });
        _autoBattleButton = _gameObjectContainer.AddToggleButton(_interfaceProvider.AutoBattleButton, ExecuteAutoBattle, 403, 563, BattleLayers.INTERFACE_LAYER, KeyboardButton.A);

        // Эти кнопки доступны всегда.
        ActivateButtons(_instantResolveButton, _autoBattleButton);

        // Эти кнопки могут быть недоступны, если первый ход - компьютера.
        if (_context.BattleState == BattleState.WaitPlayerTurn)
            ActivateButtons(_defendButton, _retreatButton, _waitButton);
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Команда текущему юниту - защититься.
    /// </summary>
    private void ExecuteDefend()
    {
        _unitActionController.Defend();
    }

    /// <summary>
    /// Команда текущему юниту - отступить.
    /// </summary>
    private void ExecuteRetreat()
    {
        // TODO
    }

    /// <summary>
    /// Команда текущему юниту - ждать.
    /// </summary>
    private void ExecuteWait()
    {
        _unitActionController.Wait();
    }

    /// <summary>
    /// Быстрое завершение битвы.
    /// </summary>
    private void ExecuteInstantBattle()
    {
        if (_context.IsInstantBattle)
            return;

        // TODO Диалог с тем, действительно ли хочет игрок быстро завершить битву.
        _context.IsInstantBattle = true;
    }

    /// <summary>
    /// Продолжить битву в автоматическом режиме.
    /// </summary>
    private void ExecuteAutoBattle()
    {
        _context.IsAutoBattle = _autoBattleButton!.IsChecked;
    }

    /// <summary>
    /// Выйти из битвы и открыть инвентарь отряда.
    /// </summary>
    private void ExecuteOpenSquadInventory()
    {
        // TODO
    }

    /// <summary>
    /// Выйти из битвы.
    /// </summary>
    private void ExecuteExit()
    {
        _gameController.ChangeScene<ILoadSagaScene, SceneParameters>(SceneParameters.Empty);
    }

    /// <summary>
    /// Активировать указанные кнопки.
    /// </summary>
    private static void ActivateButtons(params ButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetActive();
        }
    }

    /// <summary>
    /// Деактивировать указанные кнопки.
    /// </summary>
    private static void DisableButtons(params ButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetDisabled();
        }
    }

    /// <summary>
    /// Убрать кнопку со сцены.
    /// </summary>
    private static void RemoveButton(ref ButtonObject? button)
    {
        button?.Destroy();
        button = null;
    }

    /// <summary>
    /// Убрать кнопку со сцены.
    /// </summary>
    private static void RemoveButton(ref ToggleButtonObject? button)
    {
        button?.Destroy();
        button = null;
    }
}