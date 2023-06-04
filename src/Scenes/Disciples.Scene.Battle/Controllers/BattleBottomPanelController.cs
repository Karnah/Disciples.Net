using Disciples.Engine.Base;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

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
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly ITextProvider _textProvider;

    private ButtonObject _defendButton = null!;
    private ButtonObject _retreatButton = null!;
    private ButtonObject _waitButton = null!;
    private ButtonObject _instantResolveButton = null!;
    private ToggleButtonObject _autoBattleButton = null!;

    private ButtonObject _openSquadInventoryButton = null!;
    private ButtonObject _exitButton = null!;

    private BattleUnitBottomPanelData _rightUnitPanel = null!;
    private BattleUnitBottomPanelData _leftUnitPanel = null!;

    /// <summary>
    /// Создать объект типа <see cref="BattleBottomPanelController" />.
    /// </summary>
    public BattleBottomPanelController(
        IBattleGameObjectContainer gameObjectContainer,
        BattleUnitActionController unitActionController,
        BattleContext context,
        IGameController gameController,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        ITextProvider textProvider)
    {
        _gameObjectContainer = gameObjectContainer;
        _unitActionController = unitActionController;
        _context = context;
        _gameController = gameController;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _textProvider = textProvider;
    }

    /// <summary>
    /// Обработать событие изменения юнита-цели.
    /// </summary>
    public void ProcessTargetUnitChanged(BattleUnit? targetBattleUnit)
    {
        UpdateBattleUnitPanel(targetBattleUnit, _rightUnitPanel);
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

        UpdateBattleUnitPanel(_context.CurrentBattleUnit, _leftUnitPanel);
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

        // Эти кнопки доступны всегда.
        ActivateButtons(_instantResolveButton, _autoBattleButton);

        // Эти кнопки могут быть недоступны, если первый ход - компьютера.
        if (_context.BattleState == BattleState.WaitPlayerTurn)
            ActivateButtons(_defendButton, _retreatButton, _waitButton);

        _leftUnitPanel = new BattleUnitBottomPanelData
        {
            Portrait = gameObjects.Get<ImageObject>(BattleBottomPanelElementNames.LEFT_UNIT_PORTRAIT_IMAGE),
            Info = gameObjects.Get<TextBlockObject>(BattleBottomPanelElementNames.LEFT_UNIT_INFO_TEXT_BLOCK),
            LeaderItemsPanel = gameObjects.Get<ImageObject>(BattleBottomPanelElementNames.LEFT_LEADER_ITEMS_IMAGE, true)
        };
        UpdateBattleUnitPanel(_context.CurrentBattleUnit, _leftUnitPanel);

        _rightUnitPanel = new BattleUnitBottomPanelData
        {
            Portrait = gameObjects.Get<ImageObject>(BattleBottomPanelElementNames.RIGHT_UNIT_PORTRAIT_IMAGE),
            Info = gameObjects.Get<TextBlockObject>(BattleBottomPanelElementNames.RIGHT_UNIT_INFO_TEXT_BLOCK),
            LeaderItemsPanel = gameObjects.Get<ImageObject>(BattleBottomPanelElementNames.RIGHT_LEADER_ITEMS_IMAGE, true)
        };
        // На правой панели используется развёрнутое изображение.
        _rightUnitPanel.Portrait.IsReflected = true;
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
    /// Получить тестовое описание юнита.
    /// </summary>
    private TextContainer GetUnitInfoText(Unit unit)
    {
        return _textProvider
            .GetText("X100TA0608")
            .ReplacePlaceholders(new[]
            {
                ("%NAME%", new TextContainer(unit.Name)),
                ("%HP%", new TextContainer(unit.HitPoints.ToString())),
                ("%HPMAX%", new TextContainer(unit.MaxHitPoints.ToString())),
            });
    }

    /// <summary>
    /// Обновить данные юнита на выбранной панели.
    /// </summary>
    private void UpdateBattleUnitPanel(BattleUnit? battleUnit, BattleUnitBottomPanelData panelData)
    {
        if (battleUnit == null || panelData.BattleUnit == battleUnit)
            return;

        panelData.BattleUnit = battleUnit;
        panelData.Portrait.Bitmap = _battleUnitResourceProvider.GetUnitBattleFace(battleUnit.Unit.UnitType);
        panelData.Info.Text = GetUnitInfoText(battleUnit.Unit);
        panelData.LeaderItemsPanel.IsHidden = !battleUnit.Unit.IsLeader;
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