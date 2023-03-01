using Disciples.Engine.Models;
using Disciples.Scene.Battle.Models.BattleActions;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Контекст битвы.
/// </summary>
public class BattleUpdateContext
{
    private readonly List<IBattleAction> _activeActions;
    private readonly List<IBattleAction> _newActions;
    private readonly List<IBattleAction> _delayedActions;

    /// <summary>
    /// Создать объект типа <see cref="BattleUpdateContext" />.
    /// </summary>
    public BattleUpdateContext()
    {
        _activeActions = new List<IBattleAction>();
        _newActions = new List<IBattleAction>();
        _delayedActions = new List<IBattleAction>();

        InputDeviceEvents = Array.Empty<InputDeviceEvent>();
        CompletedActions = Array.Empty<IBattleAction>();
    }

    /// <summary>
    /// Количество секунд, которое прошло с предыдущего обновления сцены.
    /// </summary>
    public long TicksCount { get; private set; }

    /// <summary>
    /// Признак, что все действия были завершены.
    /// </summary>
    public bool IsAllActionsCompleted =>
        ActiveActions.Count == 0
        && NewActions.Count == 0
        && DelayedActions.Count == 0;

    /// <summary>
    /// Признак, что все действия были завершены в текущем обновлении.
    /// </summary>
    public bool IsAllActionsCompletedThisUpdate => CompletedActions.Count > 0 && IsAllActionsCompleted;

    /// <summary>
    /// Признак, что действия начались в текущем обновлении.
    /// </summary>
    public bool IsActionsBeginThisUpdate =>
        ActiveActions.Count == 0
        && CompletedActions.Count == 0
        && NewActions.Count > 0;

    /// <summary>
    /// События от устройства ввода.
    /// </summary>
    public IReadOnlyList<InputDeviceEvent> InputDeviceEvents { get; private set; }

    /// <summary>
    /// Активные действия в сражении.
    /// </summary>
    public IReadOnlyList<IBattleAction> ActiveActions => _activeActions;

    /// <summary>
    /// Новые действия, которые добавились в рамках этого обновления сцены.
    /// </summary>
    public IReadOnlyList<IBattleAction> CompletedActions { get; private set; }

    /// <summary>
    /// Новые действия, которые добавились в рамках этого обновления сцены.
    /// </summary>
    public IReadOnlyList<IBattleAction> NewActions => _newActions;

    /// <summary>
    /// Действия, которые будут запущены только тогда, когда завершатся все активные действия.
    /// </summary>
    public IReadOnlyList<IBattleAction> DelayedActions => _delayedActions;

    /// <summary>
    /// Обновить данные контекста на основе данных об обновлении сцены.
    /// </summary>
    public void BeforeSceneUpdate(UpdateSceneData updateSceneData)
    {
        TicksCount = updateSceneData.TicksCount;
        InputDeviceEvents = updateSceneData.InputDeviceEvents;

        foreach (var battleAction in _activeActions)
        {
            // Если действие завязано на времени, то обновляем счётчик.
            if (battleAction is BaseTimerBattleAction timerBattleAction)
            {
                timerBattleAction.UpdateTime(TicksCount);
            }
        }

        CompletedActions = _activeActions.Where(ba => ba.IsCompleted).ToList();
        _activeActions.RemoveAll(ba => ba.IsCompleted);

        if (_activeActions.Count == 0 && _delayedActions.Count > 0)
        {
            _newActions.AddRange(_delayedActions);
            _delayedActions.Clear();
        }
    }

    /// <summary>
    /// Обновить данные после завершения обновления сцены.
    /// </summary>
    public void AfterSceneUpdate()
    {
        _activeActions.AddRange(NewActions);
        _newActions.Clear();
    }

    /// <summary>
    /// Добавить новое действие на поле боя.
    /// </summary>
    public void AddNewAction(IBattleAction action)
    {
        _newActions.Add(action);
    }

    /// <summary>
    /// Добавить отложенное действие, которое будет выполняться после завершения всех остальных.
    /// </summary>
    public void AddDelayedAction(IBattleAction action)
    {
        _delayedActions.Add(action);
    }
}