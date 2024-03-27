namespace Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;

/// <summary>
/// Ожидание определённого промежутка времени.
/// </summary>
internal class BattleTimerDelay : IBattleActionDelay
{
    private readonly long _duration;
    private readonly Action? _onCompleted;
    private long _time;

    /// <summary>
    /// Создать объект типа <see cref="BattleTimerDelay" />.
    /// </summary>
    public BattleTimerDelay(long delay, Action? onCompletedAction = null)
    {
        _duration = delay;
        _onCompleted = onCompletedAction;
    }

    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <inheritdoc />
    public void UpdateTime(long ticks)
    {
        _time += ticks;

        if (_time >= _duration)
            IsCompleted = true;
    }

    /// <inheritdoc />
    public void ProcessCompleted()
    {
        _onCompleted?.Invoke();
    }
}