namespace Disciples.Engine.Battle.Models.BattleActions
{
    /// <summary>
    /// Базовое действие, продолжительность которого зависит от времени.
    /// </summary>
    public abstract class BaseTimerBattleAction : IBattleAction
    {
        private readonly long _duration;
        private long _time;

        /// <inheritdoc />
        protected BaseTimerBattleAction(long duration)
        {
            _duration = duration;
            _time = 0;
        }


        /// <inheritdoc />
        public bool IsEnded { get; private set; }


        /// <summary>
        /// Обновить счетчик прошедшего времени.
        /// </summary>
        public void UpdateTime(long ticks)
        {
            _time += ticks;

            if (_time >= _duration)
                IsEnded = true;
        }
    }
}