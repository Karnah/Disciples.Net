using Disciples.Engine.Extensions;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
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

    private readonly BattleActionContainer _actions = new();
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
    public bool IsCompleted { get; private set; }

    /// <inheritdoc />
    public abstract bool ShouldPassTurn { get; protected set; }

    /// <summary>
    /// Признак, что вообще в очереди никаких действий.
    /// </summary>
    private bool IsNoActions => _actions.IsNoActions;

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

        // Если после инициализации нет действий, то больше ничего делать будет не нужно.
        if (IsNoActions)
        {
            IsCompleted = true;
            OnCompleted();
        }
    }

    /// <inheritdoc />
    public void BeforeSceneUpdate()
    {
        _actions.BeforeSceneUpdate(_context.TicksCount);
    }

    /// <inheritdoc />
    public void AfterSceneUpdate()
    {
        foreach (var completedAction in _actions.Completed)
        {
            completedAction.ProcessCompleted();
        }

        _actions.AfterSceneUpdate();

        if (IsNoActions)
        {
            IsCompleted = true;

            foreach (var playingSound in _playingSounds)
            {
                playingSound.Stop();
            }

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
    /// Добавить действие.
    /// </summary>
    protected void AddAction(IBattleAction battleAction)
    {
        _actions.Add(battleAction);
    }

    /// <summary>
    /// Получить данные для отображения на портрете юнита.
    /// </summary>
    protected virtual BattleUnitPortraitEventData GetProcessorPortraitEventData(IUnitActionProcessor unitActionProcessor)
    {
        return new BattleUnitPortraitEventData(unitActionProcessor.ActionType);
    }

    /// <summary>
    /// Добавить обработчик действия.
    /// </summary>
    protected virtual void AddProcessorAction(IUnitActionProcessor unitActionProcessor)
    {
        var targetBattleUnit = _context.GetBattleUnit(unitActionProcessor.TargetUnit);
        AddProcessorAction(unitActionProcessor, targetBattleUnit, GetProcessorPortraitEventData(unitActionProcessor));
    }

    /// <summary>
    /// Добавить обработчик действия.
    /// </summary>
    private void AddProcessorAction(IUnitActionProcessor unitActionProcessor, BattleUnit targetBattleUnit, BattleUnitPortraitEventData portraitEventData)
    {
        unitActionProcessor.ProcessBeginAction();

        var targetUnitPortraitObject = _unitPortraitPanelController.GetUnitPortrait(targetBattleUnit);
        targetUnitPortraitObject?.ProcessBeginUnitPortraitEvent(portraitEventData);

        AddAction(new DelayBattleAction(COMMON_ACTION_DELAY,
            () => ProcessCompletedAction(unitActionProcessor, targetBattleUnit)));
    }

    /// <summary>
    /// Обработать начало действия.
    /// </summary>
    protected virtual void ProcessCompletedAction(IUnitActionProcessor unitActionProcessor, BattleUnit targetBattleUnit)
    {
        unitActionProcessor.ProcessCompletedAction();

        var targetUnitPortraitObject = _unitPortraitPanelController.GetUnitPortrait(targetBattleUnit);
        targetUnitPortraitObject?.ProcessCompletedUnitPortraitEvent();

        // После отображения действия на портрете, всегда есть задержка в 250 мс.
        AddAction(new DelayBattleAction(AFTER_ACTION_DELAY));
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