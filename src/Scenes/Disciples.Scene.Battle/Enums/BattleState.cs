namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Состояние битвы.
/// </summary>
internal enum BattleState
{
    /// <summary>
    /// Ожидание, когда игрок сделает ход.
    /// </summary>
    Idle,

    /// <summary>
    /// Выполнения действия юнитом.
    /// </summary>
    ProcessingAction,

    /// <summary>
    /// Ожидания выхода из битвы.
    /// </summary>
    WaitingExit
}