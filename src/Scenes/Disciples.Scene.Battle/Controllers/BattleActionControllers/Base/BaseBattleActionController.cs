using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

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
    private readonly BattleSoundController _soundController;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;

    private readonly BattleActionDelayContainer _delays = new();
    private readonly List<IPlayingSound> _playingSounds = new();

    /// <summary>
    /// Создать объект типа <see cref="BaseBattleActionController" />.
    /// </summary>
    protected BaseBattleActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer)
    {
        _context = context;
        _unitPortraitPanelController = unitPortraitPanelController;
        _soundController = soundController;
        _battleGameObjectContainer = battleGameObjectContainer;
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
        {
            // BUG Не нужно останавливать воспроизведение, так как некоторые звуки не успевают проиграться.
            foreach (var playingSound in _playingSounds)
                playingSound.Stop();

            OnCompleted();
        }
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
    /// Проиграть случайный звук.
    /// </summary>
    protected void PlayRandomSound(IReadOnlyList<RawSound> sounds)
    {
        var sound = sounds.TryGetRandomElement();
        if (sound != null)
            PlaySound(sound);
    }

    /// <summary>
    /// Проиграть звук.
    /// </summary>
    protected void PlaySound(RawSound sound)
    {
        var playingSound = _soundController.PlaySound(sound);
        _playingSounds.Add(playingSound);
    }
}