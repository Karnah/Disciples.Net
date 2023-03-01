namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие на поле боя.
/// </summary>
public interface IBattleAction
{
    /// <summary>
    /// Признак, что действие завершилось.
    /// </summary>
    bool IsCompleted { get; }
}