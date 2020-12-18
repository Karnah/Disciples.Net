namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Задержка.
    /// </summary>
    public class DelayAction : BaseTimerBattleAction
    {
        /// <inheritdoc />
        public DelayAction(long duration) : base(duration)
        { }
    }
}