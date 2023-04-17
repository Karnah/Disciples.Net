namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Список всех действий на поле боя.
/// </summary>
internal class BattleActionContainer
{
    private readonly List<IBattleAction> _activeActions;
    private readonly List<IBattleAction> _newActions;

    /// <summary>
    /// Создать объект типа <see cref="BattleActionContainer" />.
    /// </summary>
    public BattleActionContainer()
    {
        _activeActions = new List<IBattleAction>();
        _newActions = new List<IBattleAction>();

        Completed = Array.Empty<IBattleAction>();
    }

    /// <summary>
    /// Признак, что вообще в очереди никаких действий.
    /// </summary>
    public bool IsNoActions => Active.Count == 0 && New.Count == 0;

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
    /// Обновить состояние действий до начала обработки сцены.
    /// </summary>
    public void BeforeSceneUpdate(long ticksCount)
    {
        // Если действие завязано на времени, то обновляем счётчик.
        foreach (var timerBattleAction in _activeActions.OfType<BaseTimerBattleAction>())
        {
            timerBattleAction.UpdateTime(ticksCount);
        }

        Completed = _activeActions.Where(ba => ba.IsCompleted).ToArray();
        _activeActions.RemoveAll(ba => ba.IsCompleted);
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
}