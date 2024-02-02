using Disciples.Engine.Base;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Controllers.UnitActions;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контроллер для управления нижней панелью.
/// </summary>
internal class BattleBottomPanelController : BaseSupportLoading
{
    private readonly IBattleGameObjectContainer _gameObjectContainer;
    private readonly BattleUnitActionController _unitActionController;
    private readonly BattleContext _context;
    private readonly IGameController _gameController;
    private readonly IBattleInterfaceProvider _battleInterfaceProvider;

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
        IBattleGameObjectContainer gameObjectContainer,
        BattleUnitActionController unitActionController,
        BattleContext context,
        IGameController gameController,
        IBattleInterfaceProvider battleInterfaceProvider)
    {
        _gameObjectContainer = gameObjectContainer;
        _unitActionController = unitActionController;
        _context = context;
        _gameController = gameController;
        _battleInterfaceProvider = battleInterfaceProvider;
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
    public void ProcessActionsBegin()
    {
        DisableButtons(_defendButton, _retreatButton, _waitButton);
        ProcessBeginNextUnitAction();
    }

    /// <summary>
    /// Обработать начало нового действия на сцене.
    /// </summary>
    public void ProcessBeginNextUnitAction()
    {
        UpdateCurrentUnitPortrait();

        if (_context.UnitAction is MainAttackUnitAction mainAttackUnitAction)
            UpdateTargetUnitPortrait(mainAttackUnitAction.TargetBattleUnit);
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
        if (_context.BattleWinnerSquad == BattleSquadPosition.Attacker && !_context.AttackingSquad.Player.IsComputer)
        {
            _openSquadInventoryButton.IsHidden = false;
        }

        _exitButton.IsHidden = false;
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
        if (_context.BattleState != BattleState.WaitPlayerTurn)
            DisableButtons(_defendButton, _retreatButton, _waitButton);

        var battleInterfaceElements = _battleInterfaceProvider.BattleInterface.Elements;
        _leftUnitPortrait = _gameObjectContainer.AddBottomUnitPortrait(true,
            battleInterfaceElements[BattleBottomPanelElementNames.LEFT_UNIT_PORTRAIT_IMAGE],
            battleInterfaceElements[BattleBottomPanelElementNames.LEFT_LEADER_ITEMS_IMAGE],
            battleInterfaceElements[BattleBottomPanelElementNames.LEFT_UNIT_INFO_TEXT_BLOCK]);
        _leftUnitPortrait.BattleUnit = _context.CurrentBattleUnit;
        _rightUnitPortrait = _gameObjectContainer.AddBottomUnitPortrait(false,
            battleInterfaceElements[BattleBottomPanelElementNames.RIGHT_UNIT_PORTRAIT_IMAGE],
            battleInterfaceElements[BattleBottomPanelElementNames.RIGHT_LEADER_ITEMS_IMAGE],
            battleInterfaceElements[BattleBottomPanelElementNames.RIGHT_UNIT_INFO_TEXT_BLOCK]
        );
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
        _unitActionController.Retreat();
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
        _context.IsAutoBattle = _autoBattleButton.IsChecked;
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