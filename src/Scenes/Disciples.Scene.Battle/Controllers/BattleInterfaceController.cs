using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Controllers.UnitActions;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Extensions;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc cref="IBattleInterfaceController" />
internal class BattleInterfaceController : BaseSupportLoading, IBattleInterfaceController
{
    private readonly IBattleInterfaceProvider _interfaceProvider;
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly BattleUnitActionController _unitActionController;
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
        BattleUnitActionController unitActionController,
        BattleDialogController dialogController,
        BattleBottomPanelController bottomPanelController,
        ISceneInterfaceController sceneInterfaceController)
    {
        _interfaceProvider = battleInterfaceProvider;
        _context = context;
        _battleProcessor = battleProcessor;
        _sceneObjectContainer = sceneObjectContainer;
        _unitPortraitPanelController = unitPortraitPanelController;
        _unitActionController = unitActionController;
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
    /// <remarks>
    /// Порядок обработки важен.
    /// </remarks>
    public void AfterSceneUpdate()
    {
        switch (_context.BattleState)
        {
            case BattleState.BeginUnitAction:
                ProcessActionsBegin();
                break;

            case BattleState.CompletedUnitAction:
                ProcessActionsCompleted();
                break;

            case BattleState.CompletedBattle:
                ProcessBattleCompleted();
                break;
        }
    }

    /// <inheritdoc />
    public RectangleD GetBattleUnitPosition(Unit unit, BattleSquadPosition unitSquadPosition)
    {
        var squad = unitSquadPosition == BattleSquadPosition.Attacker
            ? _context.AttackingBattleSquad
            : _context.DefendingBattleSquad;
        var unitPosition = squad.GetUnitPosition(unit.SquadLinePosition, unit.SquadFlankPosition);
        if (unit.UnitType.IsSmall)
            return unitPosition;

        // Для больших юнитов необходимо пересчитать позицию.
        return unitSquadPosition == BattleSquadPosition.Attacker
            ? new RectangleD(
                unitPosition.X - 30,
                unitPosition.Y - 60,
                unitPosition.Width,
                unitPosition.Height)
            : new RectangleD(
                unitPosition.X + 35,
                unitPosition.Y - 20,
                unitPosition.Width,
                unitPosition.Height);
    }

    /// <inheritdoc />
    public void BattleUnitSelected(BattleUnit battleUnit)
    {
        // Если выбрали кости юнита, то не нужно менять портрет.
        if (battleUnit.Unit.IsDead)
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
        ShowDetailUnitInfo(battleUnit.Unit);
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
        // todo Вообще, при наведении на портреты внизу по бокам тоже нужно показывать информацию.
        // Но сейчас они позиционируются только как картинки, а не как объекты.

        ShowDetailUnitInfo(unitPortrait.Unit);
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _sceneInterfaceController.AddSceneGameObjects(_interfaceProvider.BattleInterface, Layers.SceneLayers);

        foreach (var battleground in _interfaceProvider.Battleground)
            _sceneObjectContainer.AddImage(battleground, 0, 0, BattleLayers.BACKGROUND_LAYER);

        _unitPortraitPanelController.Load();
        _bottomPanelController.Load();

        // Проверяем, если первый ход ИИ.
        _isAnimating = _context.BattleState != BattleState.WaitPlayerTurn;

        var displayingSquad = GetPanelDisplayingSquad();
        if (_isAnimating)
            _unitPortraitPanelController.DisablePanelSwitch(displayingSquad);
        else
            _unitPortraitPanelController.EnablePanelSwitch(displayingSquad);

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

        if (CanAttack(targetBattleUnit))
            _unitActionController.BeginMainAttack(targetBattleUnit);
    }

    /// <summary>
    /// Отобразить детальную информацию по указанному юниту.
    /// </summary>
    /// <param name="unit">Юнит, информацию о котором необходимо отобразить.</param>
    private void ShowDetailUnitInfo(Unit unit)
    {
        _dialogController.ShowUnitDetailInfo(unit);
    }

    /// <summary>
    /// Обработать начало действий на сцене.
    /// </summary>
    private void ProcessActionsBegin()
    {
        _isAnimating = true;

        var displayingSquad = _context.UnitAction is MainAttackUnitAction attackAction
            // Показываем отряд атакуемого юнита.
            ? attackAction.TargetBattleUnit.SquadPosition
            // Иначе это защита/ожидания и другое действие. Показываем отряд текущего юнита.
            : CurrentBattleUnit.SquadPosition;
        _unitPortraitPanelController.DisablePanelSwitch(displayingSquad);

        _bottomPanelController.ProcessActionsBegin();

        DetachSelectedAnimation();
        DetachTargetAnimations();
    }

    /// <summary>
    /// Обработать завершение всех действий на сцене.
    /// </summary>
    private void ProcessActionsCompleted()
    {
        _isAnimating = false;

        _unitPortraitPanelController.EnablePanelSwitch(GetPanelDisplayingSquad());
        _bottomPanelController.ProcessActionsCompleted();

        AttachSelectedAnimation(CurrentBattleUnit);

        if (_context.TargetBattleUnit != null && _shouldAnimateTargets)
            SelectTargetUnits();
    }

    /// <summary>
    /// Обработать завершение битвы.
    /// </summary>
    private void ProcessBattleCompleted()
    {
        _context.IsAutoBattle = false;
        _context.IsInstantBattle = false;

        _isAnimating = false;

        // Отображаем отряд победителя.
        _unitPortraitPanelController.CompleteBattle();
        _bottomPanelController.ProcessBattleCompleted();

        DetachSelectedAnimation();

        if (_context.TargetBattleUnit != null)
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
        if (_context.TargetBattleUnit == null)
            return;

        var currentUnit = CurrentBattleUnit.Unit;
        var targetUnit = _context.TargetBattleUnit.Unit;

        if (targetUnit.IsDead)
            return;

        // Если текущий юнит может атаковать только одну цель,
        // то всегда будет выделена только одна цель
        if (currentUnit.UnitType.MainAttack.Reach != UnitAttackReach.All)
        {
            AttachTargetAnimations(_context.TargetBattleUnit);
            return;
        }

        BattleUnit[] targetUnits;

        // Если юнит применяет способность на союзников (например, лекарь), то при наведении на союзника, будут выделяться все
        // Также наоборот, если юнит применяет способность на врагов, то выделятся все враги
        // Иначе, как например, лекарь при наведении на врага будет выделять только 1 врага
        if (currentUnit.Player == targetUnit.Player && currentUnit.HasAllyAbility() ||
            currentUnit.Player != targetUnit.Player && currentUnit.HasEnemyAbility())
        {
            targetUnits = BattleUnits
                .Where(u => u.Unit.Player == targetUnit.Player && u.Unit.IsDead == false)
                .ToArray();
        }
        else
        {
            targetUnits = new[] { _context.TargetBattleUnit };
        }

        AttachTargetAnimations(targetUnits);
    }

    /// <summary>
    /// Отобразить анимацию выделения цели на указанных юнитах.
    /// </summary>
    private void AttachTargetAnimations(params BattleUnit[] battleUnits)
    {
        foreach (var battleUnit in BattleUnits)
            battleUnit.IsTarget = battleUnits.Contains(battleUnit);
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
    /// Получить тип отряд, который должен отображаться на панели.
    /// </summary>
    private BattleSquadPosition GetPanelDisplayingSquad()
    {
        var currentUnit = CurrentBattleUnit.Unit;
        var showEnemies = currentUnit.HasEnemyAbility();

        return showEnemies
            ? CurrentBattleUnit.SquadPosition.GetOpposite()
            : CurrentBattleUnit.SquadPosition;
    }

    /// <summary>
    /// Проверить, может ли текущий юнит атаковать цель.
    /// </summary>
    private bool CanAttack(BattleUnit targetBattleUnit)
    {
        var attackingSquad = CurrentBattleUnit.IsAttacker
            ? _context.AttackingSquad
            : _context.DefendingSquad;
        var targetSquad = targetBattleUnit.IsAttacker
            ? _context.AttackingSquad
            : _context.DefendingSquad;
        return _battleProcessor.CanAttack(CurrentBattleUnit.Unit, attackingSquad, targetBattleUnit.Unit, targetSquad);
    }
}