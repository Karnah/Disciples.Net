using System;
using System.Collections.Generic;
using System.Diagnostics;

using Avalonia.Threading;

using Engine;

namespace AvaloniaDisciplesII
{
    public class Game : IGame
    {
        private const int TicksPerSecond = 60;

        private long _ticks;
        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;

        public Game()
        {
            GameObjects = new List<GameObject>();

            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _ticks = _stopwatch.ElapsedMilliseconds;

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1000 / TicksPerSecond) };
            _timer.Tick += UpdateScene;
            _timer.Start();
        }

        private void UpdateScene(object sender, EventArgs e)
        {
            var ticks = _stopwatch.ElapsedMilliseconds;
            var ticksCount = ticks - _ticks;
            _ticks = ticks;

            foreach (var gameObject in GameObjects) {
                foreach (var gameObjectComponent in gameObject.Components) {
                    gameObjectComponent.OnUpdate(ticksCount);
                }
            }
        }

        public IList<GameObject> GameObjects { get; private set; }
    }
}
