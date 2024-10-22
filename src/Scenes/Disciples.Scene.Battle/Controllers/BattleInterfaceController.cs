﻿using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc cref="IBattleInterfaceController" />
internal class BattleInterfaceController : BaseSupportLoading, IBattleInterfaceController
{
    private readonly IBattleInterfaceProvider _interfaceProvider;
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly BattleActionFactory _actionFactory;
    private readonly BattleDialogController _dialogController;
    private readonly BattleBottomPanelController _bottomPanelController;
    private readonly ISceneInterfaceController _sceneInterfaceController;

    /// <summary>
    /// Позволяет заблокировать действия пользователя на время анимации.
    /// </summary>
    private bool _isAnimating;

    /// <summary>
    /// Необходимо ли отрисовывать анимации цели на юнитах.
    /// </summary>
    private bool _shouldAnimateTargets;

    /// <summary>
    /// Создать объект типа <see cref="BattleInterfaceController" />.
    /// </summary>
    public BattleInterfaceController(
        IBattleInterfaceProvider battleInterfaceProvider,
        BattleContext context,
        BattleProcessor battleProcessor,
        ISceneObjectContainer sceneObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleActionFactory actionFactory,
        BattleDialogController dialogController,
        BattleBottomPanelController bottomPanelController,
        ISceneInterfaceController sceneInterfaceController)
    {
        _interfaceProvider = battleInterfaceProvider;
        _context = context;
        _battleProcessor = battleProcessor;
        _sceneObjectContainer = sceneObjectContainer;
        _unitPortraitPanelController = unitPortraitPanelController;
        _actionFactory = actionFactory;
        _dialogController = dialogController;
        _bottomPanelController = bottomPanelController;
        _sceneInterfaceController = sceneInterfaceController;
    }

    /// <summary>
    ///  Юнит, который выполняет свой ход.
    /// </summary>
    private BattleUnit CurrentBattleUnit => _context.CurrentBattleUnit;

    /// <summary>
    /// Список юнитов.
    /// </summary>
    private IReadOnlyList<BattleUnit> BattleUnits => _context.BattleUnits;

    /// <inheritdoc />
    public void PreLoad()
    {
        _context.AttackingBattleSquad.UnitPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattlegroundElementNames.ATTACK_UNIT_PATTERN_PLACEHOLDER);
        _context.DefendingBattleSquad.UnitPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattlegroundElementNames.DEFEND_UNIT_PATTERN_PLACEHOLDER);
    }

    /// <inheritdoc />
    public void BeforeSceneUpdate()
    {
        _unitPortraitPanelController.ProcessMousePosition(_context.MousePosition);
    }

    /// <inheritdoc />
    public void AfterSceneUpdate()
    {
        switch (_context.BattleActionEvent)
        {
            case BattleActionEvent.ActionBegin:
                ProcessActionBegin();
                break;

            case BattleActionEvent.ActionCompleted:
                ProcessActionCompleted();
                break;

            case BattleActionEvent.BattleCompleted:
                ProcessBattleCompleted();
                break;
        }
    }

    /// <inheritdoc />
    public RectangleD GetBattleUnitPosition(BattleSquadPosition squadPosition, UnitSquadPosition unitPosition)
    {
        var squad = squadPosition == BattleSquadPosition.Attacker
            ? _context.AttackingBattleSquad
            : _context.DefendingBattleSquad;
        var battleUnitPosition = squad.GetUnitPosition(unitPosition.Line, unitPosition.Flank);
        if (unitPosition.Line != UnitSquadLinePosition.Both)
            return battleUnitPosition;

        // Для больших юнитов необходимо пересчитать позицию.
        return squadPosition == BattleSquadPosition.Attacker
            ? new RectangleD(
                battleUnitPosition.X - 30,
                battleUnitPosition.Y - 60,
                battleUnitPosition.Width,
                battleUnitPosition.Height)
            : new RectangleD(
                battleUnitPosition.X + 35,
                battleUnitPosition.Y - 20,
                battleUnitPosition.Width,
                battleUnitPosition.Height);
    }

    #region События от игровых объектов

    /// <inheritdoc />
    public void BattleUnitSelected(BattleUnit battleUnit)
    {
        // Если выбрали кости юнита, то не нужно менять портрет.
        if (battleUnit.Unit.IsInactive)
            return;

        UpdateTargetUnit(battleUnit);
    }

    /// <inheritdoc />
    public void BattleUnitUnselected(BattleUnit battleUnit)
    {
        UpdateTargetUnit(null);
    }

    /// <inheritdoc />
    public void BattleUnitLeftMouseButtonClicked(BattleUnit battleUnit)
    {
        Attack(battleUnit);
    }

    /// <inheritdoc />
    public void BattleUnitRightMouseButtonPressed(BattleUnit battleUnit)
    {
        // Если юнит сбежал, то информацию по нему будет показана только по портрету.
        if (battleUnit.Unit.IsRetreated)
            return;

        ShowDetailUnitInfo(battleUnit.Unit);
    }

    /// <inheritdoc />
    public void SummonPlaceholderLeftMouseButtonClicked(SummonPlaceholder summonPlaceholder)
    {
        if (_isAnimating)
            return;

        _actionFactory.BeginMainAttack(summonPlaceholder.SquadPosition, summonPlaceholder.UnitPosition);
    }

    /// <inheritdoc />
    public void SummonPlaceholderRightMouseButtonPressed(SummonPlaceholder summonPlaceholder)
    {
        // Находим юнита, который перекрыт плейсхолдером. Выводим информацию по нему.
        var hiddenUnit = _context
            .GetBattleUnits(summonPlaceholder.SquadPosition, summonPlaceholder.UnitPosition)
            .FirstOrDefault();
        if (hiddenUnit != null)
            BattleUnitRightMouseButtonPressed(hiddenUnit);
    }

    /// <inheritdoc />
    public void UnitPortraitSelected(UnitPortraitObject unitPortrait)
    {
        var targetBattleUnit = _context.GetBattleUnit(unitPortrait.Unit);
        UpdateTargetUnit(targetBattleUnit, false);
    }

    /// <inheritdoc />
    public void UnitPortraitLeftMouseButtonClicked(UnitPortraitObject unitPortrait)
    {
        var targetBattleUnit = _context.GetBattleUnit(unitPortrait.Unit);
        Attack(targetBattleUnit);
    }

    /// <inheritdoc />
    public void UnitPortraitRightMouseButtonPressed(UnitPortraitObject unitPortrait)
    {
        ShowDetailUnitInfo(unitPortrait.Unit);
    }

    /// <inheritdoc />
    public void BottomUnitPortraitRightMouseButtonPressed(BottomUnitPortraitObject bottomUnitPortrait)
    {
        var unit = bottomUnitPortrait.BattleUnit?.Unit;
        if (unit != null)
            ShowDetailUnitInfo(unit);
    }

    /// <summary>
    /// Отобразить детальную информацию по указанному юниту.
    /// </summary>
    /// <param name="unit">Юнит, информацию о котором необходимо отобразить.</param>
    private void ShowDetailUnitInfo(Unit unit)
    {
        _dialogController.ShowUnitDetailInfo(unit);
    }

    #endregion

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _sceneInterfaceController.AddSceneGameObjects(_interfaceProvider.BattleInterface, Layers.SceneLayers);

        foreach (var battleground in _interfaceProvider.Battleground)
            _sceneObjectContainer.AddImage(battleground, 0, 0, BattleLayers.BACKGROUND_LAYER);

        _unitPortraitPanelController.Load();
        _bottomPanelController.Load();

        // Проверяем, если первый ход ИИ.
        _isAnimating = _context.BattleState != BattleState.Idle;

        AttachSelectedAnimation(CurrentBattleUnit);
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        _unitPortraitPanelController.Unload();
        _bottomPanelController.Unload();
    }

    /// <summary>
    /// Обновить цель.
    /// </summary>
    /// <param name="targetUnitObject">Юнит, на которого навели курсором.</param>
    /// <param name="shouldAnimateTarget">Необходимо ли выделить юнита с помощью анимации (красный крутящийся круг).</param>
    private void UpdateTargetUnit(BattleUnit? targetUnitObject, bool shouldAnimateTarget = true)
    {
        _context.TargetBattleUnit = targetUnitObject;
        _bottomPanelController.ProcessTargetUnitChanged(targetUnitObject);

        if (targetUnitObject == null)
        {
            DetachTargetAnimations();
            return;
        }

        _shouldAnimateTargets = shouldAnimateTarget;

        if (shouldAnimateTarget && !_isAnimating)
            SelectTargetUnits();
    }

    /// <summary>
    /// Атаковать указанного юнита.
    /// </summary>
    private void Attack(BattleUnit targetBattleUnit)
    {
        if (_isAnimating)
            return;

        if (_battleProcessor.CanAttack(targetBattleUnit.Unit))
        {
            _actionFactory.BeginMainAttack(targetBattleUnit.SquadPosition, targetBattleUnit.Unit.Position);
            return;
        }

        // Проверяем, если текущий юнит имеет массовую атаку,
        // И может атаковать других юнитов в том же отряде.
        var currentUnit = CurrentBattleUnit.Unit;
        var mainAttack = currentUnit.MainAttack;
        var alternativeAttack = currentUnit.AlternativeAttack;
        var secondaryAttack = currentUnit.SecondaryAttack;

        var newTargetUnit = GetAttackBattleUnit(targetBattleUnit, mainAttack, secondaryAttack);
        if (newTargetUnit == null && alternativeAttack != null)
            newTargetUnit = GetAttackBattleUnit(targetBattleUnit, alternativeAttack, secondaryAttack);

        if (newTargetUnit != null)
            _actionFactory.BeginMainAttack(newTargetUnit.SquadPosition, newTargetUnit.Unit.Position);
    }

    /// <summary>
    /// Обработать начало действий на сцене.
    /// </summary>
    private void ProcessActionBegin()
    {
        _isAnimating = true;

        _unitPortraitPanelController.ProcessActionBegin();
        _bottomPanelController.ProcessActionBegin();

        DetachSelectedAnimation();
        DetachTargetAnimations();
    }

    /// <summary>
    /// Обработать завершение всех действий на сцене.
    /// </summary>
    private void ProcessActionCompleted()
    {
        _isAnimating = false;

        _unitPortraitPanelController.ProcessActionCompleted();
        _bottomPanelController.ProcessActionCompleted();

        AttachSelectedAnimation(CurrentBattleUnit);

        if (_shouldAnimateTargets)
            SelectTargetUnits();
    }

    /// <summary>
    /// Обработать завершение битвы.
    /// </summary>
    private void ProcessBattleCompleted()
    {
        _context.IsAutoBattle = false;
        _context.IsInstantBattleRequested = false;

        _isAnimating = false;

        _unitPortraitPanelController.ProcessBattleCompleted();
        _bottomPanelController.ProcessBattleCompleted();

        DetachSelectedAnimation();
        SelectTargetUnits();
    }


    /// <summary>
    /// Отобразить анимацию выделения на текущем юните.
    /// </summary>
    private void AttachSelectedAnimation(BattleUnit battleUnit)
    {
        DetachSelectedAnimation();

        if (_isAnimating)
            return;

        battleUnit.IsUnitTurn = true;
    }

    /// <summary>
    /// Скрыть анимацию выделения на текущем юните.
    /// </summary>
    private void DetachSelectedAnimation()
    {
        var selectedBattleUnit = BattleUnits.FirstOrDefault(bu => bu.IsUnitTurn);
        if (selectedBattleUnit != null)
            selectedBattleUnit.IsUnitTurn = false;
    }

    /// <summary>
    /// Отобразить анимацию выделения цели исходя из типа атаки текущего юнита.
    /// </summary>
    private void SelectTargetUnits()
    {
        var targetBattleUnit = _context.TargetBattleUnit;
        if (targetBattleUnit == null)
            return;

        if (targetBattleUnit.Unit.IsInactive)
        {
            targetBattleUnit.IsTarget = false;
            return;
        }

        // Выбранному юниту всегда проставляем выделение,
        // Даже если его нельзя атаковать.
        targetBattleUnit.IsTarget = true;

        // После завершения битвы всегда отображается только один юнит.
        if (_context.BattleState == BattleState.WaitingExit)
            return;

        var targetBattleUnits = _battleProcessor
            .GetMainAttackTargetUnits(targetBattleUnit.Unit)
            .Where(u => u != targetBattleUnit.Unit)
            .Select(_context.GetBattleUnit);
        foreach (var battleUnit in targetBattleUnits)
            battleUnit.IsTarget = true;
    }

    /// <summary>
    /// Скрыть анимации выделения цели на всех юнитах.
    /// </summary>
    private void DetachTargetAnimations()
    {
        foreach (var battleUnit in BattleUnits)
            battleUnit.IsTarget = false;
    }

    /// <summary>
    /// Получить юнит-цель.
    /// </summary>
    /// <remarks>
    /// Если текущий юнит имеет атаку "по всем", то можно нажать на юнита, которого он не может атаковать.
    /// В этом случае, эффект будет применён ко все юнитам, которых можно атаковать.
    /// </remarks>
    private BattleUnit? GetAttackBattleUnit(BattleUnit targetBattleUnit, CalculatedUnitAttack mainAttack, CalculatedUnitAttack? secondaryAttack)
    {
        if (mainAttack.Reach != UnitAttackReach.All)
            return null;

        return _battleProcessor
            .GetUnitSquad(targetBattleUnit.Unit)
            .Units
            // targetBattleUnit уже проверили, его точно нельзя атаковать.
            .Where(u => u != targetBattleUnit.Unit && _battleProcessor.CanAttack(u, mainAttack, secondaryAttack))
            .Select(_context.GetBattleUnit)
            .FirstOrDefault();
    }
}