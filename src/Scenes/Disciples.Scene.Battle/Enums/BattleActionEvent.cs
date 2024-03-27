namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Событие, которое произошло в битве.
/// </summary>
internal enum BattleActionEvent
{
    /// <summary>
    /// Нет события.
    /// </summary>
    None,

    /// <summary>
    /// Началось новое действие.
    /// </summary>
    ActionBegin,

    /// <summary>
    /// Действие завершилось.
    /// </summary>
    ActionCompleted,

    /// <summary>
    /// Битва завершилась.
    /// </summary>
    BattleCompleted
}