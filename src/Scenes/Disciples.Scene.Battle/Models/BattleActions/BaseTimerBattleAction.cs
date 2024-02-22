namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Базовое действие, продолжительность которого зависит от времени.
/// </summary>
internal abstract class BaseTimerBattleAction : IBattleAction
{
    private readonly long _duration;
    private readonly Action? _onCompleted;
    private long _time;

    /// <summary>
    /// Создать объект типа <see cref="BaseTimerBattleAction" />.
    /// </summary>
    protected BaseTimerBattleAction(long duration, Action? onCompleted = null)
    {
        _duration = duration;
        _onCompleted = onCompleted;
        _time = 0;
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