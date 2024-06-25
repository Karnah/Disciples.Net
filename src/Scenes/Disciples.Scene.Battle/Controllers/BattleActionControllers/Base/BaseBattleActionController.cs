using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;

/// <summary>
/// Класс для базовых действий на поле боя.
/// </summary>
internal abstract class BaseBattleActionController : IBattleActionController
{
    protected const long COMMON_ACTION_DELAY = 1000;
    protected const long SMALL_ACTION_DELAY = 250;

    private readonly BattleContext _context;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly BattleBottomPanelController _bottomPanelController;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleUnitResourceProvider _unitResourceProvider;

    private readonly BattleActionDelayContainer _delays = new();

    /// <summary>
    /// Создать объект типа <see cref="BaseBattleActionController" />.
    /// </summary>
    protected BaseBattleActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleBottomPanelController bottomPanelController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider)
    {
        _context = context;
        _unitPortraitPanelController = unitPortraitPanelController;
        _bottomPanelController = bottomPanelController;
        _battleGameObjectContainer = battleGameObjectContainer;
        _unitResourceProvider = unitResourceProvider;
    }

    /// <inheritdoc />
    public bool IsCompleted => _delays.IsCompleted;

    /// <inheritdoc />
    public abstract bool ShouldPassTurn { get; protected set; }

    /// <summary>
    /// Текущий юнит.
    /// </summary>
    protected BattleUnit CurrentBattleUnit => _context.CurrentBattleUnit;

    /// <inheritdoc />
    public void Initialize()
    {
        // Плесхолдеры для вызываемых юнитов создаются в начале хода юнита-призывателя.
        // Завершение хода (т.е. любое следующее действие) должно их уничтожить.
        if (_context.SummonPlaceholders.Count > 0)
        {
            foreach (var summonPlaceholder in _context.SummonPlaceholders)
                summonPlaceholder.Destroy();

            _context.SummonPlaceholders.Clear();

            // Из-за плейсхолдеров часть юнитов могли быть заблокированы для выделения.
            // Возвращаем это обратно.
            foreach (var battleUnit in _context.BattleUnits)
                battleUnit.IsSelectionEnabled = true;
        }

        var targetSquadPosition = GetTargetSquadPosition();
        if (targetSquadPosition != null)
            _unitPortraitPanelController.SetDisplayingSquad(targetSquadPosition.Value);

        InitializeInternal();

        // Если после инициализации ничего ожидать не нужно,
        // То действие сразу завершается.
        if (IsCompleted)
        {
            OnCompleted();
            return;
        }

        _context.BattleState = BattleState.ProcessingAction;
        _context.BattleActionEvent = BattleActionEvent.ActionBegin;
    }

    /// <inheritdoc />
    public void BeforeSceneUpdate()
    {
        _delays.BeforeSceneUpdate(_context.TicksCount);
    }

    /// <inheritdoc />
    public void AfterSceneUpdate()
    {
        foreach (var completedAction in _delays.Completed)
        {
            completedAction.ProcessCompleted();
        }

        _delays.AfterSceneUpdate();

        if (IsCompleted)
            OnCompleted();
    }

    /// <summary>
    /// Получить отряд, который должен отображаться на панели.
    /// </summary>
    protected virtual BattleSquadPosition? GetTargetSquadPosition() => null;

    /// <summary>
    /// Инициализировать действие.
    /// </summary>
    protected abstract void InitializeInternal();

    /// <summary>
    /// Обработать завершения всех действий.
    /// </summary>
    protected virtual void OnCompleted()
    {
        _context.BattleState = _context.NextAction == null
            ? BattleState.Idle
            : BattleState.ProcessingAction;
        _context.BattleActionEvent = BattleActionEvent.ActionCompleted;
    }

    /// <summary>
    /// Добавить ожидание действия.
    /// </summary>
    protected void AddActionDelay(IBattleActionDelay battleAction)
    {
        _delays.Add(battleAction);
    }

    /// <summary>
    /// Начать обработку действия.
    /// </summary>
    protected virtual void AddProcessorAction(IUnitActionProcessor unitActionProcessor)
    {
        unitActionProcessor.ProcessBeginAction();

        var targetBattleUnit = _context.GetBattleUnit(unitActionProcessor.TargetUnit);
        var portraitMessage = GetProcessorPortraitEventData(unitActionProcessor);
        _unitPortraitPanelController.DisplayMessage(targetBattleUnit, portraitMessage);

        AddActionDelay(new BattleTimerDelay(COMMON_ACTION_DELAY,
            () => OnProcessorActionCompleted(unitActionProcessor, targetBattleUnit)));
    }

    /// <summary>
    /// Обработать завершение действия.
    /// </summary>
    protected virtual void OnProcessorActionCompleted(IUnitActionProcessor unitActionProcessor, BattleUnit targetBattleUnit)
    {
        unitActionProcessor.ProcessCompletedAction();

        _unitPortraitPanelController.CloseMessage(targetBattleUnit);

        // После отображения действия на портрете, всегда есть задержка в 250 мс.
        AddActionDelay(new BattleTimerDelay(SMALL_ACTION_DELAY));
    }

    /// <summary>
    /// Получить данные для отображения на портрете юнита.
    /// </summary>
    protected virtual BattleUnitPortraitEventData GetProcessorPortraitEventData(IUnitActionProcessor unitActionProcessor)
    {
        return new BattleUnitPortraitEventData(unitActionProcessor.ActionType);
    }

    /// <summary>
    /// Добавить юнита на поле боя.
    /// </summary>
    protected BattleUnit AddBattleUnit(Unit unit, BattleSquadPosition squadPosition)
    {
        var battleUnit = _battleGameObjectContainer.AddBattleUnit(unit, squadPosition);

        _context.BattleUnits.Add(battleUnit);
        _unitPortraitPanelController.ProcessBattleUnitUpdated(battleUnit);
        _bottomPanelController.ProcessBattleUnitUpdated(battleUnit);

        // Удаляем юнитов, которые перекрываются вызванным.
        if (battleUnit.Unit is SummonedUnit summonedUnit)
        {
            var hiddenUnits = summonedUnit
                .HiddenUnits
                .Select(_context.TryGetBattleUnit)
                .Where(hu => hu != null)
                .Select(hu => hu!);
            foreach (var hiddenUnit in hiddenUnits)
                RemoveBattleUnit(hiddenUnit);
        }

        return battleUnit;
    }

    /// <summary>
    /// Заменить юнита на поле боя.
    /// </summary>
    protected void ReplaceUnit(BattleUnit originalBattleUnit, Unit newUnit)
    {
        originalBattleUnit.Destroy();

        var newBattleUnit = _battleGameObjectContainer.AddBattleUnit(newUnit, originalBattleUnit.SquadPosition);
        newBattleUnit.UnitState = originalBattleUnit.UnitState;
        var unitBattleIndex = _context.BattleUnits.IndexOf(originalBattleUnit);
        _context.BattleUnits[unitBattleIndex] = newBattleUnit;

        // BUG: Опасная штука, так как эффекты для юнита рассчитываются заранее.
        // Но у всех трансформированных юнитов общий список эффектов.
        if (_context.CurrentBattleUnit == originalBattleUnit)
            _context.CurrentBattleUnit = newBattleUnit;

        var targetUnitPortrait = _unitPortraitPanelController.GetUnitPortrait(originalBattleUnit);
        targetUnitPortrait?.ChangeUnit(newUnit);
    }

    /// <summary>
    /// Удалить юнита с поля боя.
    /// </summary>
    protected void RemoveBattleUnit(BattleUnit targetBattleUnit)
    {
        targetBattleUnit.Destroy();

        _context.BattleUnits.Remove(targetBattleUnit);
        _unitPortraitPanelController.ProcessBattleUnitUpdated(targetBattleUnit);
        _bottomPanelController.ProcessBattleUnitUpdated(targetBattleUnit);

        // Если юнит призванный, то возвращаем на место тех юнитов, что он скрывал.
        if (targetBattleUnit.Unit is SummonedUnit summonedUnit)
        {
            foreach (var hiddenUnit in summonedUnit.HiddenUnits)
            {
                // Если вызванный юнит маленький и перекрывает большого юнита, то нужно проверить вторую клетку в линии.
                // Если там тоже призванный юнит, то пока возвращать исходного юнита на место нельзя.
                if (summonedUnit.UnitType.IsSmall && !hiddenUnit.UnitType.IsSmall)
                {
                    var otherLinePosition = summonedUnit.SquadPosition.GetOtherLine();
                    var otherSummonedBattleUnit = _context.GetBattleUnits(new BattleUnitPosition(targetBattleUnit.SquadPosition, otherLinePosition)).FirstOrDefault();
                    if (otherSummonedBattleUnit != null)
                        continue;
                }

                var battleUnit = AddBattleUnit(hiddenUnit, targetBattleUnit.SquadPosition);
                if (hiddenUnit.IsDead)
                    battleUnit.UnitState = BattleUnitState.Dead;
                else if (hiddenUnit.IsRetreated)
                    battleUnit.UnitState = BattleUnitState.Retreated;
            }
        }
    }

    /// <summary>
    /// Добавить анимацию удаления призванного юнита.
    /// </summary>
    protected void AddUnitUnsummonAnimationAction(BattleUnit targetBattleUnit)
    {
        var animationPoint = targetBattleUnit.AnimationComponent.AnimationPoint;
        var unsummonAnimationFrames = targetBattleUnit.Unit.UnitType.IsSmall
            ? _unitResourceProvider.SmallUnitUnsummonAnimationFrames
            : _unitResourceProvider.BigUnitUnsummonAnimationFrames;
        var unitUnsummonAnimation = _battleGameObjectContainer.AddAnimation(
            unsummonAnimationFrames,
            animationPoint.X,
            animationPoint.Y,
            targetBattleUnit.AnimationComponent.Layer + 2,
            false);
        AddActionDelay(new BattleAnimationDelay(unitUnsummonAnimation.AnimationComponent));
    }
}