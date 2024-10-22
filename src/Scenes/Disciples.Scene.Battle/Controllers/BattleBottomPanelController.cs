﻿using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Settings;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Controllers.BattleActionControllers;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контроллер для управления нижней панелью.
/// </summary>
internal class BattleBottomPanelController : BaseSupportLoading
{
    private readonly GameSettings _gameSettings;
    private readonly IBattleGameObjectContainer _gameObjectContainer;
    private readonly BattleActionFactory _actionFactory;
    private readonly BattleContext _context;
    private readonly IGameController _gameController;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;
    private readonly BattleDialogController _dialogController;
    private readonly ITextProvider _textProvider;
    private readonly BattleProcessor _battleProcessor;

    private ButtonObject _defendButton = null!;
    private ButtonObject _retreatButton = null!;
    private ButtonObject _waitButton = null!;
    private ButtonObject _instantResolveButton = null!;
    private ToggleButtonObject _autoBattleButton = null!;

    private ButtonObject _openSquadInventoryButton = null!;
    private ButtonObject _exitButton = null!;

    private BottomUnitPortraitObject _rightUnitPortrait = null!;
    private BottomUnitPortraitObject _leftUnitPortrait = null!;

    /// <summary>
    /// Создать объект типа <see cref="BattleBottomPanelController" />.
    /// </summary>
    public BattleBottomPanelController(
        GameSettings gameSettings,
        IBattleGameObjectContainer gameObjectContainer,
        BattleActionFactory actionFactory,
        BattleContext context,
        IGameController gameController,
        IBattleInterfaceProvider battleInterfaceProvider,
        BattleDialogController dialogController,
        ITextProvider textProvider,
        BattleProcessor battleProcessor)
    {
        _gameSettings = gameSettings;
        _gameObjectContainer = gameObjectContainer;
        _actionFactory = actionFactory;
        _context = context;
        _gameController = gameController;
        _battleInterfaceProvider = battleInterfaceProvider;
        _dialogController = dialogController;
        _textProvider = textProvider;
        _battleProcessor = battleProcessor;
    }

    /// <summary>
    /// Обработать событие изменения юнита-цели.
    /// </summary>
    public void ProcessTargetUnitChanged(BattleUnit? targetBattleUnit)
    {
        UpdateTargetUnitPortrait(targetBattleUnit);
    }

    /// <summary>
    /// Обработать начало действий на сцене.
    /// </summary>
    public void ProcessActionBegin()
    {
        DisableButtons(_defendButton, _retreatButton, _waitButton);
        ProcessBeginNextAction();
    }

    /// <summary>
    /// Обработать начало нового действия на сцене.
    /// </summary>
    public void ProcessBeginNextAction()
    {
        UpdateCurrentUnitPortrait();

        if (_context.Action is MainAttackActionController mainAttackUnitAction)
        {
            var targetUnit = _context.GetBattleUnits(mainAttackUnitAction.TargetSquadPosition, mainAttackUnitAction.TargetUnitPosition).FirstOrDefault();
            UpdateTargetUnitPortrait(targetUnit);
        }
    }

    /// <summary>
    /// Обработать завершение всех действий на сцене.
    /// </summary>
    public void ProcessActionCompleted()
    {
        // Если юнит наносит второй удар, то указанные кнопки активировать не нужно.
        if (!_context.IsSecondAttack)
        {
            ActivateButtons(_defendButton, _retreatButton);

            // Если юнит уже ждал на этом ходу, то больше ждать не может.
            if (!_battleProcessor.UnitTurnQueue.IsWaitingUnitTurn)
                ActivateButtons(_waitButton);
        }

        UpdateCurrentUnitPortrait();
    }

    /// <summary>
    /// Обработать завершение битвы.
    /// </summary>
    public void ProcessBattleCompleted()
    {
        _defendButton.IsHidden = true;
        _retreatButton.IsHidden = true;
        _waitButton.IsHidden = true;
        _instantResolveButton.IsHidden = true;
        _autoBattleButton.IsHidden = true;

        // Возможность открыть инвентарь будет только, если победил игрок при атаке.
        if (_battleProcessor.AttackingSquad == _battleProcessor.WinnerSquad && !_battleProcessor.AttackingSquad.Player.IsComputer)
        {
            _openSquadInventoryButton.IsHidden = _gameSettings.IsUselessButtonsHidden;
        }

        _exitButton.IsHidden = false;
    }

    /// <summary>
    /// Обработать изменение состояния юнита.
    /// Это может быть удаление, обновление или трансформация.
    /// </summary>
    public void ProcessBattleUnitUpdated(BattleUnit battleUnit)
    {
        // Таким образом мы можем как найти нового юнита (например, после трансформации),
        // Или не найти совсем (после удаления).
        var newBattleUnit = _context.TryGetBattleUnit(battleUnit.Unit);

        if (_leftUnitPortrait.BattleUnit == battleUnit)
            _leftUnitPortrait.BattleUnit = newBattleUnit;

        if (_rightUnitPortrait.BattleUnit == battleUnit)
            _rightUnitPortrait.BattleUnit = newBattleUnit;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var gameObjects = _gameObjectContainer.GameObjects;

        _defendButton = gameObjects.GetButton(BattleBottomPanelElementNames.DEFEND_BUTTON, ExecuteDefend);
        _retreatButton = gameObjects.GetButton(BattleBottomPanelElementNames.RETREAT_BUTTON, ExecuteRetreat);
        _waitButton = gameObjects.GetButton(BattleBottomPanelElementNames.WAIT_BUTTON, ExecuteWait);
        _instantResolveButton = gameObjects.GetButton(BattleBottomPanelElementNames.INSTANT_RESOLVE_BUTTON, ExecuteInstantBattle);
        _autoBattleButton = gameObjects.GetToggleButton(BattleBottomPanelElementNames.AUTO_BATTLE_TOGGLE_BUTTON, ExecuteAutoBattle);

        _exitButton = gameObjects.GetButton(BattleBottomPanelElementNames.EXIT_BUTTON, ExecuteExit, true);
        _openSquadInventoryButton = gameObjects.GetButton(BattleBottomPanelElementNames.SQUAD_INVENTORY_BUTTON, ExecuteOpenSquadInventory, true);

        // Эти кнопки недоступны, если первый ход - компьютера.
        if (_context.BattleState != BattleState.Idle)
            DisableButtons(_defendButton, _retreatButton, _waitButton);

        var battleInterfaceElements = _battleInterfaceProvider.BattleInterface.Elements;
        _leftUnitPortrait = _gameObjectContainer.AddBottomUnitPortrait(true,
            battleInterfaceElements[BattleBottomPanelElementNames.LEFT_UNIT_PORTRAIT_IMAGE],
            battleInterfaceElements[BattleBottomPanelElementNames.LEFT_LEADER_ITEMS_IMAGE],
            battleInterfaceElements[BattleBottomPanelElementNames.LEFT_UNIT_INFO_TEXT_BLOCK]);
        _rightUnitPortrait = _gameObjectContainer.AddBottomUnitPortrait(false,
            battleInterfaceElements[BattleBottomPanelElementNames.RIGHT_UNIT_PORTRAIT_IMAGE],
            battleInterfaceElements[BattleBottomPanelElementNames.RIGHT_LEADER_ITEMS_IMAGE],
            battleInterfaceElements[BattleBottomPanelElementNames.RIGHT_UNIT_INFO_TEXT_BLOCK]
        );
        UpdateCurrentUnitPortrait();
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
        _actionFactory.Defend();
    }

    /// <summary>
    /// Команда текущему юниту - отступить.
    /// </summary>
    private void ExecuteRetreat()
    {
        _actionFactory.Retreat();
    }

    /// <summary>
    /// Команда текущему юниту - ждать.
    /// </summary>
    private void ExecuteWait()
    {
        _actionFactory.Wait();
    }

    /// <summary>
    /// Быстрое завершение битвы.
    /// </summary>
    private void ExecuteInstantBattle()
    {
        if (_context.IsInstantBattleRequested)
            return;

        _dialogController.ShowConfirm(_textProvider.GetText("X005TA0922"), () =>
        {
            _context.IsInstantBattleRequested = true;
        });
    }

    /// <summary>
    /// Продолжить битву в автоматическом режиме.
    /// </summary>
    private void ExecuteAutoBattle()
    {
        _context.IsAutoBattle = _autoBattleButton.IsChecked;
    }

    /// <summary>
    /// Выйти из битвы и открыть инвентарь отряда.
    /// </summary>
    private void ExecuteOpenSquadInventory()
    {
    }

    /// <summary>
    /// Выйти из битвы.
    /// </summary>
    private void ExecuteExit()
    {
        if (_context.GameContext.MissionType == MissionType.Saga)
            _gameController.ChangeScene<ILoadSagaScene, SceneParameters>(SceneParameters.Empty);
        else
            _gameController.ChangeScene<ILoadQuestScene, SceneParameters>(SceneParameters.Empty);
    }

    /// <summary>
    /// Обновить портрет текущего юнита.
    /// </summary>
    private void UpdateCurrentUnitPortrait()
    {
        var portrait = _context.CurrentBattleUnit.IsAttacker
            ? _leftUnitPortrait
            : _rightUnitPortrait;
        portrait.BattleUnit = _context.CurrentBattleUnit;
    }

    /// <summary>
    /// Обновить портрет юнита-цели.
    /// </summary>
    private void UpdateTargetUnitPortrait(BattleUnit? battleUnit)
    {
        if (battleUnit == null)
            return;

        var portrait = _context.CurrentBattleUnit.IsAttacker
            ? _rightUnitPortrait
            : _leftUnitPortrait;
        portrait.BattleUnit = battleUnit;
    }

    /// <summary>
    /// Активировать указанные кнопки.
    /// </summary>
    private static void ActivateButtons(params BaseButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetActive();
        }
    }

    /// <summary>
    /// Деактивировать указанные кнопки.
    /// </summary>
    private static void DisableButtons(params BaseButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetDisabled();
        }
    }
}