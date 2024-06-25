using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers;

/// <summary>
/// Контроллер завершения битвы.
/// </summary>
internal class CompleteBattleActionController : BaseBattleActionController
{
    private readonly BattleContext _context;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleProcessor _battleProcessor;
    private readonly IBattleUnitResourceProvider _unitResourceProvider;
    private readonly BattleSoundController _soundController;

    /// <summary>
    /// Создать объект типа <see cref="CompleteBattleActionController" />.
    /// </summary>
    public CompleteBattleActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleBottomPanelController bottomPanelController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleProcessor battleProcessor,
        BattleSoundController soundController
        ) : base(context, unitPortraitPanelController, bottomPanelController, battleGameObjectContainer, unitResourceProvider)
    {
        _context = context;
        _unitPortraitPanelController = unitPortraitPanelController;
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleProcessor = battleProcessor;
        _unitResourceProvider = unitResourceProvider;
        _soundController = soundController;
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; }

    /// <inheritdoc />
    protected override BattleSquadPosition? GetTargetSquadPosition()
    {
        return _context.WinnerSquadPosition!.Value;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        _unitPortraitPanelController.DisableBorderAnimations();

        var processors = _battleProcessor.CompleteBattle();
        foreach (var unitExperienceProcessor in processors)
            AddProcessorAction(unitExperienceProcessor);
    }

    /// <inheritdoc />
    protected override void OnCompleted()
    {
        base.OnCompleted();

        _context.BattleState = BattleState.WaitingExit;
        _context.BattleActionEvent = BattleActionEvent.BattleCompleted;
    }

    /// <inheritdoc />
    protected override void AddProcessorAction(IUnitActionProcessor unitActionProcessor)
    {
        if (unitActionProcessor is UnitCompleteBattleProcessor unitCompleteBattleProcessor)
        {
            AddUnitCompleteBattleProcessorAction(unitCompleteBattleProcessor);
            return;
        }

        base.AddProcessorAction(unitActionProcessor);
    }

    /// <summary>
    /// Добавить обработчик опыта.
    /// </summary>
    private void AddUnitCompleteBattleProcessorAction(UnitCompleteBattleProcessor unitCompleteBattleProcessor)
    {
        unitCompleteBattleProcessor.ProcessBeginAction();

        var targetBattleUnit = _context.GetBattleUnit(unitCompleteBattleProcessor.TargetUnit);
        _unitPortraitPanelController.DisplayMessage(targetBattleUnit,
            new BattleUnitPortraitEventData(UnitActionType.Experience));

        // Если юнит не вырос в уровне, то сразу и завершаем обработку.
        // Также не снимаем с портрета событие, оно будет отображаться до конца сцены.
        if (unitCompleteBattleProcessor.LevelUpUnit == null)
        {
            unitCompleteBattleProcessor.ProcessCompletedAction();
            return;
        }

        var targetUnitPortrait = _unitPortraitPanelController.GetUnitPortrait(targetBattleUnit);
        targetUnitPortrait?.ChangeUnit(unitCompleteBattleProcessor.LevelUpUnit);

        AddUnitLevelUpAnimationAction(targetBattleUnit);
        _soundController.PlayUnitLevelUpSound();

        AddActionDelay(new BattleTimerDelay(SMALL_ACTION_DELAY,
            () => OnUnitCompleteBattleProcessorActionCompleted(unitCompleteBattleProcessor)));
    }

    /// <summary>
    /// Обработать завершение действия.
    /// </summary>
    private void OnUnitCompleteBattleProcessorActionCompleted(UnitCompleteBattleProcessor unitCompleteBattleProcessor)
    {
        unitCompleteBattleProcessor.ProcessCompletedAction();

        var targetBattleUnit = _context.GetBattleUnit(unitCompleteBattleProcessor.TargetUnit);
        _unitPortraitPanelController.CloseMessage(targetBattleUnit);

        ReplaceUnit(targetBattleUnit, unitCompleteBattleProcessor.LevelUpUnit!);
    }

    /// <summary>
    /// Добавить анимацию для повышения уровня юнитом.
    /// </summary>
    private void AddUnitLevelUpAnimationAction(BattleUnit targetBattleUnit)
    {
        var animationPoint = targetBattleUnit.AnimationComponent.AnimationPoint;
        var unitLevelUpAnimation = _battleGameObjectContainer.AddAnimation(
            _unitResourceProvider.GetUnitLevelUpAnimation(targetBattleUnit.Unit.UnitType),
            animationPoint.X,
            animationPoint.Y,
            targetBattleUnit.AnimationComponent.Layer + 2,
            false);
        AddActionDelay(new BattleAnimationDelay(unitLevelUpAnimation.AnimationComponent));
    }
}