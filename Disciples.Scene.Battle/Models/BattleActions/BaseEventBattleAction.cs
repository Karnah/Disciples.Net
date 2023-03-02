namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Базовый тип для событий на поле боя.
/// Необходимо, чтобы список действий использовать как шину событий.
///
/// TODO Переделать на отдельный класс событий?
/// </summary>
internal abstract class BaseEventBattleAction : IBattleAction
{
    /// <inheritdoc />
    public bool IsCompleted => true;
}