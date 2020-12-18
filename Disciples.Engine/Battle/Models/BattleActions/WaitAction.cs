namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Действие, которое завершается тогда, когда завершены все прочие действия.
    /// </summary>
    public abstract class WaitAction : IBattleAction
    {
        /// <inheritdoc />
        public bool IsEnded => false;
    }
}