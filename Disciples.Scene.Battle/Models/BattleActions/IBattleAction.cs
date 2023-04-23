namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие на поле боя.
/// </summary>
internal interface IBattleAction
{
    /// <summary>
    /// Признак, что действие завершилось.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Обновить счетчик прошедшего времени.
    /// </summary>
    void UpdateTime(long ticks);
}