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

        private readonly LinkedList<GameObject> _gameObjects;

        private long _ticks;
        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;

        public Game()
        {
            _gameObjects = new LinkedList<GameObject>();

            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _ticks = _stopwatch.ElapsedMilliseconds;

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1000 / TicksPerSecond) };
            _timer.Tick += UpdateScene;
            _timer.Start();
        }

        private void UpdateScene(object sender, EventArgs args)
        {
            var ticks = _stopwatch.ElapsedMilliseconds;
            var ticksCount = ticks - _ticks;
            _ticks = ticks;

            try {
                for (var gameObjectNode = _gameObjects.First; gameObjectNode != null; ) {
                    var nextNode = gameObjectNode.Next;

                    if (gameObjectNode.Value.IsDestroyed) {
                        _gameObjects.Remove(gameObjectNode);
                    }
                    else {
                        gameObjectNode.Value.OnUpdate(ticksCount);
                    }

                    gameObjectNode = nextNode;
                }
            }
            catch (Exception e) {
                // todo Обрабатывать это с помощью логов
                Console.WriteLine(e);
            }
        }

        public IReadOnlyCollection<GameObject> GameObjects => _gameObjects;


        public void CreateObject(GameObject gameObject)
        {
            gameObject.OnInitialize();
            _gameObjects.AddLast(gameObject);
        }

        public void DestroyObject(GameObject gameObject)
        {
            gameObject.Destroy();
        }


        public void ClearScene()
        {
            _gameObjects.Clear();
        }
    }
}
