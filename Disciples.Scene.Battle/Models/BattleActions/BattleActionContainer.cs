namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Список всех действий на поле боя.
/// </summary>
internal class BattleActionContainer
{
    private readonly List<IBattleAction> _activeActions;
    private readonly List<IBattleAction> _newActions;
    private readonly List<IBattleAction> _delayedActions;

    /// <summary>
    /// Создать объект типа <see cref="BattleActionContainer" />.
    /// </summary>
    public BattleActionContainer()
    {
        _activeActions = new List<IBattleAction>();
        _newActions = new List<IBattleAction>();
        _delayedActions = new List<IBattleAction>();

        Completed = Array.Empty<IBattleAction>();
    }

    /// <summary>
    /// Признак, что все действия были завершены.
    /// </summary>
    public bool IsAllActionsCompleted =>
        Active.Count == 0
        && New.Count == 0
        && Delayed.Count == 0;

    /// <summary>
    /// Признак, что все действия были завершены в текущем обновлении.
    /// </summary>
    public bool IsAllActionsCompletedThisUpdate => Completed.Count > 0 && IsAllActionsCompleted;

    /// <summary>
    /// Признак, что действия начались в текущем обновлении.
    /// </summary>
    public bool IsActionsBeginThisUpdate =>
        Active.Count == 0
        && Completed.Count == 0
        && New.Count > 0;

    /// <summary>
    /// Активные действия в сражении.
    /// </summary>
    public IReadOnlyList<IBattleAction> Active => _activeActions;

    /// <summary>
    /// Новые действия, которые добавились в рамках этого обновления сцены.
    /// </summary>
    public IReadOnlyList<IBattleAction> Completed { get; private set; }

    /// <summary>
    /// Новые действия, которые добавились в рамках этого обновления сцены.
    /// </summary>
    public IReadOnlyList<IBattleAction> New => _newActions;

    /// <summary>
    /// Действия, которые будут запущены только тогда, когда завершатся все активные действия.
    /// </summary>
    public IReadOnlyList<IBattleAction> Delayed => _delayedActions;

    /// <summary>
    /// Обновить состояние действий до начала обработки сцены.
    /// </summary>
    public void BeforeSceneUpdate(long ticksCount)
    {
        foreach (var battleAction in _activeActions)
        {
            // Если действие завязано на времени, то обновляем счётчик.
            if (battleAction is BaseTimerBattleAction timerBattleAction)
                timerBattleAction.UpdateTime(ticksCount);
        }

        Completed = _activeActions.Where(ba => ba.IsCompleted).ToList();
        _activeActions.RemoveAll(ba => ba.IsCompleted);

        if (_activeActions.Count == 0 && _delayedActions.Count > 0)
        {
            _newActions.AddRange(_delayedActions);
            _delayedActions.Clear();
        }
    }

    /// <summary>
    /// Обновить состояние действий после обработки сцены.
    /// </summary>
    public void AfterSceneUpdate()
    {
        _activeActions.AddRange(New);
        _newActions.Clear();
    }

    /// <summary>
    /// Добавить новое действие на поле боя.
    /// </summary>
    public void Add(IBattleAction action)
    {
        _newActions.Add(action);
    }

    /// <summary>
    /// Добавить отложенное действие, которое будет выполняться после завершения всех остальных.
    /// </summary>
    public void AddDelayed(IBattleAction action)
    {
        _delayedActions.Add(action);
    }
}