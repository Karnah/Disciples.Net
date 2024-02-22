namespace Disciples.Scene.Battle.Controllers.UnitActionControllers.Models;

/// <summary>
/// Ожидание перед выполнением какого-то действия.
/// </summary>
internal interface IBattleActionDelay
{
    /// <summary>
    /// Признак, что ожидание завершилось.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Обновить счетчик прошедшего времени.
    /// </summary>
    void UpdateTime(long ticks);

    /// <summary>
    /// Обработать завершение действия.
    /// </summary>
    void ProcessCompleted();
}