using Disciples.Engine.Extensions;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.Controllers.UnitActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

namespace Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;

/// <summary>
/// Класс для базовых действий юнита.
/// </summary>
internal abstract class BaseUnitActionController : IBattleUnitActionController
{
    protected const long COMMON_ACTION_DELAY = 1000;
    private const long AFTER_ACTION_DELAY = 250;

    private readonly BattleContext _context;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly BattleSoundController _soundController;

    private readonly BattleActionDelayContainer _delays = new();
    private readonly List<IPlayingSound> _playingSounds = new();

    /// <summary>
    /// Создать объект типа <see cref="BaseUnitActionController" />.
    /// </summary>
    protected BaseUnitActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController)
    {
        _context = context;
        _unitPortraitPanelController = unitPortraitPanelController;
        _soundController = soundController;
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
        _unitPortraitPanelController.ProcessActionsBegin(targetSquadPosition);

        InitializeInternal();

        // Если после инициализации ничего ожидать не нужно,
        // То действие сразу завершается.
        if (IsCompleted)
            OnCompleted();
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
    protected abstract BattleSquadPosition GetTargetSquadPosition();

    /// <summary>
    /// Инициализировать действие.
    /// </summary>
    protected abstract void InitializeInternal();

    /// <summary>
    /// Обработать завершения всех действий.
    /// </summary>
    protected virtual void OnCompleted()
    {
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
        var targetUnitPortraitObject = _unitPortraitPanelController.GetUnitPortrait(targetBattleUnit);
        var portraitEventData = GetProcessorPortraitEventData(unitActionProcessor);
        targetUnitPortraitObject?.ProcessBeginUnitPortraitEvent(portraitEventData);

        AddActionDelay(new BattleTimerDelay(COMMON_ACTION_DELAY,
            () => OnProcessorActionCompleted(unitActionProcessor, targetBattleUnit)));
    }

    /// <summary>
    /// Обработать завершение действия.
    /// </summary>
    protected virtual void OnProcessorActionCompleted(IUnitActionProcessor unitActionProcessor, BattleUnit targetBattleUnit)
    {
        unitActionProcessor.ProcessCompletedAction();

        var targetUnitPortraitObject = _unitPortraitPanelController.GetUnitPortrait(targetBattleUnit);
        targetUnitPortraitObject?.ProcessCompletedUnitPortraitEvent();

        // После отображения действия на портрете, всегда есть задержка в 250 мс.
        AddActionDelay(new BattleTimerDelay(AFTER_ACTION_DELAY));
    }

    /// <summary>
    /// Получить данные для отображения на портрете юнита.
    /// </summary>
    protected virtual BattleUnitPortraitEventData GetProcessorPortraitEventData(IUnitActionProcessor unitActionProcessor)
    {
        return new BattleUnitPortraitEventData(unitActionProcessor.ActionType);
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