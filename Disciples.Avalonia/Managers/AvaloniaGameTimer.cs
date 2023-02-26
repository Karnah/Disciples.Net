using System;
using Avalonia.Threading;
using Disciples.Engine.Platform.Managers;

namespace Disciples.Avalonia.Managers
{
    /// <inheritdoc />
    public class AvaloniaGameTimer : IGameTimer
    {
        /// <summary>
        /// Количество вызовов таймера в секунду.
        /// </summary>
        private const int TICKS_PER_SECOND = 60;

        /// <summary>
        /// Объект таймера.
        /// </summary>
        private DispatcherTimer? _dispatcherTimer;

        /// <inheritdoc />
        public event EventHandler? TimerTick;

        /// <inheritdoc />
        public void Start()
        {
            _dispatcherTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1000 / TICKS_PER_SECOND) };
            _dispatcherTimer.Tick += OnTimerTick;
            _dispatcherTimer.Start();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _dispatcherTimer?.Stop();
            _dispatcherTimer = null;
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            TimerTick?.Invoke(sender, EventArgs.Empty);
        }
    }
}