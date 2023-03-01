namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Базовое действие, продолжительность которого зависит от времени.
/// </summary>
public abstract class BaseTimerBattleAction : IBattleAction
{
    private readonly long _duration;
    private long _time;

    /// <summary>
    /// Создать объект типа <see cref="BaseTimerBattleAction" />.
    /// </summary>
    protected BaseTimerBattleAction(long duration)
    {
        _duration = duration;
        _time = 0;
    }

    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Обновить счетчик прошедшего времени.
    /// </summary>
    public void UpdateTime(long ticks)
    {
        _time += ticks;

        if (_time >= _duration)
            IsCompleted = true;
    }
}