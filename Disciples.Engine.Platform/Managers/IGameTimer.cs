using System;

namespace Disciples.Engine.Platform.Managers
{
    /// <summary>
    /// Игровой таймер.
    /// </summary>
    public interface IGameTimer
    {
        /// <summary>
        /// Событие срабатывания таймера.
        /// </summary>
        event EventHandler TimerTick;


        /// <summary>
        /// Начать работу таймера.
        /// </summary>
        void Start();

        /// <summary>
        /// Остановить таймер.
        /// </summary>
        void Stop();
    }
}