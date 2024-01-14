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
    /// Завершение одного действия и переход к следующему.
    /// </summary>
    BeginNextUnitAction,

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