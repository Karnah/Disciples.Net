using System;

namespace Engine.Models
{
    /// <summary>
    /// Аргументы событий, связанных с перерисовкой сцены.
    /// </summary>
    public class SceneUpdatingEventArgs : EventArgs
    {
        /// <inheritdoc />
        public SceneUpdatingEventArgs(long ticksCount)
        {
            TicksCount = ticksCount;
        }

        /// <summary>
        /// Количество тиков, которое прошло с момента предыдущего вызова.
        /// </summary>
        public long TicksCount { get; }
    }
}