namespace Disciples.Scene.Battle.Controllers.UnitActionControllers.Models;

/// <summary>
/// Список всех событий ожидания на поле боя.
/// </summary>
internal class BattleActionDelayContainer
{
    private readonly List<IBattleActionDelay> _activeDelays;
    private readonly List<IBattleActionDelay> _newDelays;

    /// <summary>
    /// Создать объект типа <see cref="BattleActionDelayContainer" />.
    /// </summary>
    public BattleActionDelayContainer()
    {
        _activeDelays = new List<IBattleActionDelay>();
        _newDelays = new List<IBattleActionDelay>();

        Completed = Array.Empty<IBattleActionDelay>();
    }

    /// <summary>
    /// Признак, что завершены все ожидания.
    /// </summary>
    public bool IsCompleted => Active.Count == 0 && New.Count == 0;

    /// <summary>
    /// Активные ожидания в сражении.
    /// </summary>
    public IReadOnlyList<IBattleActionDelay> Active => _activeDelays;

    /// <summary>
    /// Завершенные ожидания в рамках этого обновления сцены.
    /// </summary>
    public IReadOnlyList<IBattleActionDelay> Completed { get; private set; }

    /// <summary>
    /// Новые ожидания, которые добавились в рамках этого обновления сцены.
    /// </summary>
    public IReadOnlyList<IBattleActionDelay> New => _newDelays;

    /// <summary>
    /// Обновить состояние ожиданий до начала обработки сцены.
    /// </summary>
    public void BeforeSceneUpdate(long ticksCount)
    {
        foreach (var battleAction in _activeDelays)
        {
            battleAction.UpdateTime(ticksCount);
        }

        Completed = _activeDelays.Where(ba => ba.IsCompleted).ToArray();
        _activeDelays.RemoveAll(ba => ba.IsCompleted);
    }

    /// <summary>
    /// Обновить состояние ожиданий после обработки сцены.
    /// </summary>
    public void AfterSceneUpdate()
    {
        _activeDelays.AddRange(New);
        _newDelays.Clear();
    }

    /// <summary>
    /// Добавить новое ожидание.
    /// </summary>
    public void Add(IBattleActionDelay actionDelay)
    {
        _newDelays.Add(actionDelay);
    }
}