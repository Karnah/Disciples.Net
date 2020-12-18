namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Действие на поле боя.
    /// </summary>
    public interface IBattleAction
    {
        /// <summary>
        /// Было ли завершено действие.
        /// </summary>
        bool IsEnded { get; }
    }
}