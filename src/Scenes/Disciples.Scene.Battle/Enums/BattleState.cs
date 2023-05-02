namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Состояние битвы.
/// </summary>
internal enum BattleState
{
    /// <summary>
    /// Ожидание, когда игрок сделает ход.
    /// </summary>
    WaitPlayerTurn,

    /// <summary>
    /// Начало действия юнитом.
    /// </summary>
    BeginUnitAction,

    /// <summary>
    /// Выполнения действия юнитом.
    /// </summary>
    ProcessingUnitAction,

    /// <summary>
    /// Завершения всех действий.
    /// </summary>
    CompletedUnitAction,

    /// <summary>
    /// Завершение битвы.
    /// </summary>
    CompletedBattle,

    /// <summary>
    /// Ожидания выхода из битвы.
    /// </summary>
    WaitExit
}